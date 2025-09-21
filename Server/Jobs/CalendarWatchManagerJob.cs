using Dev1.Module.GoogleAdmin.Services;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Jobs
{
    public class CalendarWatchManagerJob : HostedServiceBase
    {

        public CalendarWatchManagerJob(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory)
        {
            Name = "Dev1 Google Calendar Watch Manager";
            // Run every 5 minutes
            Frequency = "m";
            Interval = 1;
            IsEnabled = true;
        }


        public override async Task<string> ExecuteJobAsync(IServiceProvider provider)
        {
            string log = "";
            var now = DateTime.UtcNow;
            var renewBefore = now.AddMinutes(45);

            var _siteRepo = provider.GetRequiredService<ISiteRepository>();
            var _watchService = provider.GetRequiredService<ICalendarWatchService>();



            // iterate through sites for current tenant
            List<Site> sites = _siteRepo.GetSites().ToList();
            foreach (Site site in sites)
            {
                //var siteIds = aliases?.Select(a => a.SiteId).Distinct().ToList() ?? [];

                //int totalRenewed = 0, totalCleaned = 0;

                //foreach (var siteId in siteIds)
                //{
                //if (cancellationToken.IsCancellationRequested) break;

                var renewed = await _watchService.RenewExpiringAsync(site.SiteId, renewBefore);
                var cleaned = await _watchService.CleanupAsync(site.SiteId, now);

                //totalRenewed += renewed;
                //totalCleaned += cleaned;
            }

            return log;
        }
    }
}
