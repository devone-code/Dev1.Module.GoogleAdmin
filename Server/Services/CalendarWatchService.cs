using System;
using System.Linq;
using System.Threading.Tasks;
using Dev1.Module.GoogleAdmin.Repository;
using Dev1.Module.GoogleAdmin.Shared.Models;
using Google.Apis.Calendar.v3;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Linq.Expressions;
using Oqtane.Shared;

namespace Dev1.Module.GoogleAdmin.Services
{
    // Server-side implementation; exposed via the shared interface
    public class ServerCalendarWatchService : ICalendarWatchService, ITransientService
    {
        private readonly IDbContextFactory<GoogleAdminContext> _factory;
        private readonly IGoogleCredentials _googleCredentials;
        private readonly ITenantManager _tenantManager;
        private readonly ISettingRepository _settings;
        private readonly ILogManager _logger;

        public ServerCalendarWatchService(
            IDbContextFactory<GoogleAdminContext> factory,
            IGoogleCredentials googleCredentials,
            ITenantManager tenantManager,
            ISettingRepository settings,
            ILogManager logger)
        {
            _factory = factory;
            _googleCredentials = googleCredentials;
            _tenantManager = tenantManager;
            _settings = settings;
            _logger = logger;
        }

        public async Task<CalendarWatch> EnsureWatchAsync(int siteId, string calendarId, string userEmail, string webhookUrl)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var watch = await db.CalendarWatch
                .Where(x => x.SiteId == siteId && x.CalendarId == calendarId && x.UserEmail == userEmail)
                .FirstOrDefaultAsync();

            if (watch == null)
            {
                watch = new CalendarWatch
                {
                    SiteId = siteId,
                    CalendarId = calendarId,
                    UserEmail = userEmail,
                    RefCount = 0,
                    ExpirationUtc = DateTime.UtcNow.AddMinutes(30),
                    TokenKey = Guid.NewGuid().ToString("N"),
                    WebhookUrl = webhookUrl
                };
                db.CalendarWatch.Add(watch);
            }

            // Always keep the latest webhookUrl
            if (!string.Equals(watch.WebhookUrl, webhookUrl, StringComparison.Ordinal))
            {
                watch.WebhookUrl = webhookUrl;
            }

            watch.RefCount++;

            // (Re)create if not present or near expiry
            if (string.IsNullOrEmpty(watch.ChannelId) || string.IsNullOrEmpty(watch.ResourceId) || watch.ExpirationUtc <= DateTime.UtcNow.AddMinutes(60))
            {
                await CreateOrRenewGoogleWatchAsync(watch);
            }

            await db.SaveChangesAsync();
            return watch;
        }

        public async Task DecrementAsync(int siteId, string calendarId, string userEmail)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var watch = await db.CalendarWatch
                .Where(x => x.SiteId == siteId && x.CalendarId == calendarId && x.UserEmail == userEmail)
                .FirstOrDefaultAsync();
            if (watch == null) return;

            if (watch.RefCount > 0) watch.RefCount--;
            await db.SaveChangesAsync();
        }

        public async Task<int> RenewExpiringAsync(int siteId, DateTime renewBeforeUtc)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var expiring = await db.CalendarWatch
                .Where(x => x.SiteId == siteId && x.RefCount > 0 && x.ExpirationUtc <= renewBeforeUtc)
                .ToListAsync();

            foreach (var w in expiring)
            {
                try
                {
                    await CreateOrRenewGoogleWatchAsync(w);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, Oqtane.Enums.LogFunction.Other,
                        "Failed to renew watch {WatchId} {Calendar} Site {SiteId}: {Error}", w.CalendarWatchId, w.CalendarId, siteId, ex.Message);
                }
            }

            await db.SaveChangesAsync();
            return expiring.Count;
        }

        public async Task<int> CleanupAsync(int siteId, DateTime nowUtc)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var stale = await db.CalendarWatch
                .Where(x => x.SiteId == siteId && (x.RefCount == 0 || x.ExpirationUtc <= nowUtc))
                .ToListAsync();

            foreach (var w in stale)
            {
                try
                {
                    await StopGoogleChannelAsync(w);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warning, this, Oqtane.Enums.LogFunction.Delete,
                        "Failed stopping Google channel for watch {WatchId}: {Error}", w.CalendarWatchId, ex.Message);
                }
                db.CalendarWatch.Remove(w);
            }

            await db.SaveChangesAsync();
            return stale.Count;
        }

        public async Task<CalendarWatch> GetByTokenAsync(string tokenKey)
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.CalendarWatch.AsNoTracking().FirstOrDefaultAsync(x => x.TokenKey == tokenKey);
        }

        private async Task CreateOrRenewGoogleWatchAsync(CalendarWatch watch)
        {
            var scopes = new[] { CalendarService.Scope.CalendarReadonly };
            ICredential credential = string.IsNullOrEmpty(watch.UserEmail)
                ? _googleCredentials.GetServiceAccountCredential(scopes)
                : await _googleCredentials.GetUserGoogleCredentialAsync(scopes, watch.UserEmail);

            var service = new CalendarService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Oqtane Google Calendar Module"
            });

            var address = watch.WebhookUrl; // Use persisted webhook URL

            var channel = new Google.Apis.Calendar.v3.Data.Channel
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = "web_hook",
                Address = address,
                Token = watch.TokenKey
            };

            var watchRequest = service.Events.Watch(channel, watch.CalendarId);
            var response = await watchRequest.ExecuteAsync();

            watch.ChannelId = response.Id;
            watch.ResourceId = response.ResourceId;
            if (response.Expiration.HasValue)
            {
                var epoch = DateTime.UnixEpoch.AddMilliseconds(response.Expiration.Value);
                watch.ExpirationUtc = DateTime.SpecifyKind(epoch, DateTimeKind.Utc);
            }
            else
            {
                watch.ExpirationUtc = DateTime.UtcNow.AddHours(1);
            }
        }

        private async Task StopGoogleChannelAsync(CalendarWatch watch)
        {
            if (string.IsNullOrEmpty(watch.ChannelId) || string.IsNullOrEmpty(watch.ResourceId))
                return;

            var scopes = new[] { CalendarService.Scope.CalendarReadonly };
            ICredential credential = string.IsNullOrEmpty(watch.UserEmail)
                ? _googleCredentials.GetServiceAccountCredential(scopes)
                : await _googleCredentials.GetUserGoogleCredentialAsync(scopes, watch.UserEmail);

            var service = new CalendarService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Oqtane Google Calendar Module"
            });

            // channels.stop
            var channel = new Google.Apis.Calendar.v3.Data.Channel
            {
                Id = watch.ChannelId,
                ResourceId = watch.ResourceId
            };
            await service.Channels.Stop(channel).ExecuteAsync();
        }


    }
}
