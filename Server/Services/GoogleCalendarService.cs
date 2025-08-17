using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using Oqtane.Repository;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Auth.OAuth2;
using Dev1.Module.GoogleAdmin.Models;
using System.Collections.Generic;
using Dev1.Module.GoogleAdmin.Shared.Models;
using System.Text.Json;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class ServerGoogleCalendarService : IGoogleCalendarService
    {
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Alias _alias;
        private readonly IGoogleCredentials _googleCredentials;
        private readonly ISettingRepository _settingRepository;

        public ServerGoogleCalendarService(
            IUserPermissions userPermissions,
            ITenantManager tenantManager,
            ISettingRepository settingRepository,
            IGoogleCredentials googleCredentials,
            ILogManager logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userPermissions = userPermissions;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _alias = tenantManager.GetAlias();
            _settingRepository = settingRepository;
            _googleCredentials = googleCredentials;
        }

        public async Task<CalendarAuthInfo> GetCalendarAuthInfoAsync(int moduleId, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return new CalendarAuthInfo { ErrorMessage = "Unauthorized access." };
            }

            // If no userEmail provided, try to get current user's email
            userEmail ??= GetCurrentUserEmail();

            return await _googleCredentials.GetAuthInfoAsync(userEmail);
        }

        public async Task<CalendarList> GetAvailableGoogleCalendarsAsync(int moduleId, CalendarAuthMode authMode, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                throw new UnauthorizedAccessException("Unauthorized access to module.");
            }

            try
            {
                // Get user email for impersonation if needed
                userEmail = GetUserEmailForAuthMode(authMode, userEmail);

                var calendarService = await CreateCalendarServiceAsync(authMode, userEmail);
                var calendars = await calendarService.CalendarList.List().ExecuteAsync();
                return calendars;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error getting calendars with {AuthMode} for user {UserEmail}: {Error}", authMode, userEmail ?? "unknown", ex.Message);
                throw;
            }
        }

        public async Task<CalendarList> GetAvailableGoogleCalendarsAsync(int moduleId, string impersonateAccount)
        {
            var authMode = string.IsNullOrEmpty(impersonateAccount) ? CalendarAuthMode.OrganizationCalendar : CalendarAuthMode.UserCalendar;
            return await GetAvailableGoogleCalendarsAsync(moduleId, authMode, impersonateAccount);
        }

        public async Task<Calendar> GetGoogleCalendarAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                throw new UnauthorizedAccessException("Unauthorized access to module.");
            }

            try
            {
                userEmail = GetUserEmailForAuthMode(authMode, userEmail);

                var calendarService = await CreateCalendarServiceAsync(authMode, userEmail);
                var calendar = await calendarService.Calendars.Get(calendarId).ExecuteAsync();
                return calendar;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error getting calendar {CalendarId} with {AuthMode} for user {UserEmail}: {Error}", calendarId, authMode, userEmail ?? "unknown", ex.Message);
                throw;
            }
        }

        public async Task<Events> GetCalendarEventsAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                throw new UnauthorizedAccessException("Unauthorized access to module.");
            }

            try
            {
                userEmail = GetUserEmailForAuthMode(authMode, userEmail);

                var calendarService = await CreateCalendarServiceAsync(authMode, userEmail);
                var events = await calendarService.Events.List(calendarId).ExecuteAsync();
                return events;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error getting events for calendar {CalendarId} with {AuthMode} for user {UserEmail}: {Error}", calendarId, authMode, userEmail ?? "unknown", ex.Message);
                throw;
            }
        }

        public async Task<string> ScheduleCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode,
            string timezone, DateTime startDate, DateTime endDate, string summary,
            string attendeeName, string attendeeEmail, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                throw new UnauthorizedAccessException("Unauthorized to edit calendar events.");
            }

            try
            {
                userEmail = GetUserEmailForAuthMode(authMode, userEmail);

                var calendarService = await CreateCalendarServiceAsync(authMode, userEmail);

                var calendarEvent = new Event
                {
                    Summary = summary,
                    Start = new EventDateTime
                    {
                        DateTimeDateTimeOffset = startDate,
                        TimeZone = timezone
                    },
                    End = new EventDateTime
                    {
                        DateTimeDateTimeOffset = endDate,
                        TimeZone = timezone
                    },
                    Attendees = new List<EventAttendee>
                    {
                        new EventAttendee
                        {
                            DisplayName = attendeeName,
                            Email = attendeeEmail
                        }
                    }
                };

                var createdEvent = await calendarService.Events.Insert(calendarEvent, calendarId).ExecuteAsync();
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Calendar event created: {EventId} for user {UserEmail}", createdEvent.Id, userEmail ?? "unknown");

                return createdEvent.HtmlLink;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error creating calendar event with {AuthMode} for user {UserEmail}: {Error}", authMode, userEmail ?? "unknown", ex.Message);
                throw;
            }
        }

        public async Task<string> ScheduleCalendarEventAsync(int moduleId, string impersonateAccount, string calendarId,
            string timezone, DateTime startDate, DateTime endDate, string summary,
            string attendeeName, string attendeeEmail)
        {
            var authMode = string.IsNullOrEmpty(impersonateAccount) ? CalendarAuthMode.OrganizationCalendar : CalendarAuthMode.UserCalendar;
            return await ScheduleCalendarEventAsync(moduleId, calendarId, authMode, timezone, startDate, endDate, summary, attendeeName, attendeeEmail, impersonateAccount);
        }

        public async Task<string> CreateExtendedCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, ExtendedAppointment appointment, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                throw new UnauthorizedAccessException("Unauthorized to edit calendar events.");
            }

            try
            {
                userEmail = GetUserEmailForAuthMode(authMode, userEmail);

                var calendarService = await CreateCalendarServiceAsync(authMode, userEmail);

                var calendarEvent = new Event
                {
                    Summary = appointment.Text,
                    Description = appointment.Description,
                    Location = appointment.Location,
                    Start = new EventDateTime
                    {
                        DateTimeDateTimeOffset = appointment.Start,
                        TimeZone = appointment.Timezone
                    },
                    End = new EventDateTime
                    {
                        DateTimeDateTimeOffset = appointment.End,
                        TimeZone = appointment.Timezone
                    }
                };

                // Add attendees if any
                if (appointment.AttendeeEmails?.Any() == true)
                {
                    calendarEvent.Attendees = appointment.AttendeeEmails.Select(email => new EventAttendee
                    {
                        Email = email
                    }).ToList();
                }

                var createdEvent = await calendarService.Events.Insert(calendarEvent, calendarId).ExecuteAsync();
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Extended calendar event created: {EventId} for user {UserEmail}", createdEvent.Id, userEmail ?? "unknown");

                return createdEvent.Id; // Return the event ID for future updates
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error creating extended calendar event with {AuthMode} for user {UserEmail}: {Error}", authMode, userEmail ?? "unknown", ex.Message);
                throw;
            }
        }

        public async Task<string> UpdateCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, ExtendedAppointment appointment, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                throw new UnauthorizedAccessException("Unauthorized to edit calendar events.");
            }

            if (string.IsNullOrEmpty(appointment.GoogleEventId))
            {
                throw new ArgumentException("Event ID is required for updates.");
            }

            try
            {
                userEmail = GetUserEmailForAuthMode(authMode, userEmail);

                var calendarService = await CreateCalendarServiceAsync(authMode, userEmail);

                // Get the existing event first
                var existingEvent = await calendarService.Events.Get(calendarId, appointment.GoogleEventId).ExecuteAsync();

                // Update the event properties
                existingEvent.Summary = appointment.Text;
                existingEvent.Description = appointment.Description;
                existingEvent.Location = appointment.Location;
                existingEvent.Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = appointment.Start,
                    TimeZone = appointment.Timezone
                };
                existingEvent.End = new EventDateTime
                {
                    DateTimeDateTimeOffset = appointment.End,
                    TimeZone = appointment.Timezone
                };

                // Update attendees
                if (appointment.AttendeeEmails?.Any() == true)
                {
                    existingEvent.Attendees = appointment.AttendeeEmails.Select(email => new EventAttendee
                    {
                        Email = email
                    }).ToList();
                }
                else
                {
                    existingEvent.Attendees = null;
                }

                var updatedEvent = await calendarService.Events.Update(existingEvent, calendarId, appointment.GoogleEventId).ExecuteAsync();
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Calendar event updated: {EventId} for user {UserEmail}", updatedEvent.Id, userEmail ?? "unknown");

                return updatedEvent.Id;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error updating calendar event {EventId} with {AuthMode} for user {UserEmail}: {Error}", appointment.GoogleEventId, authMode, userEmail ?? "unknown", ex.Message);
                throw;
            }
        }

        public async Task DeleteCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string eventId, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                throw new UnauthorizedAccessException("Unauthorized to edit calendar events.");
            }

            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentException("Event ID is required for deletion.");
            }

            try
            {
                userEmail = GetUserEmailForAuthMode(authMode, userEmail);

                var calendarService = await CreateCalendarServiceAsync(authMode, userEmail);
                await calendarService.Events.Delete(calendarId, eventId).ExecuteAsync();

                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Calendar event deleted: {EventId} for user {UserEmail}", eventId, userEmail ?? "unknown");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error deleting calendar event {EventId} with {AuthMode} for user {UserEmail}: {Error}", eventId, authMode, userEmail ?? "unknown", ex.Message);
                throw;
            }
        }

        public async Task<string> GetCalendarAccessLevelAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                throw new UnauthorizedAccessException("Unauthorized access to module.");
            }

            try
            {
                userEmail = GetUserEmailForAuthMode(authMode, userEmail);

                var calendarService = await CreateCalendarServiceAsync(authMode, userEmail);
                var calendarList = await calendarService.CalendarList.List().ExecuteAsync();

                var calendar = calendarList.Items?.FirstOrDefault(c => c.Id == calendarId);
                return calendar?.AccessRole ?? "none";
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error getting calendar access level for {CalendarId} with {AuthMode} for user {UserEmail}: {Error}", calendarId, authMode, userEmail ?? "unknown", ex.Message);
                return "none";
            }
        }

        private async Task<CalendarService> CreateCalendarServiceAsync(CalendarAuthMode authMode, string userEmail)
        {
            ICredential credential;
            string applicationName = GetApplicationName();

            switch (authMode)
            {
                case CalendarAuthMode.OrganizationCalendar:
                    // ServiceAccountCredential implements ICredential
                    credential = _googleCredentials.GetServiceAccountCredential(new[] { CalendarService.Scope.Calendar });
                    break;

                case CalendarAuthMode.UserCalendar:
                    // GoogleCredential also implements ICredential - pass userEmail for impersonation
                    credential = await _googleCredentials.GetUserGoogleCredentialAsync(new[] { CalendarService.Scope.Calendar }, userEmail);
                    break;

                //case CalendarAuthMode.OAuth2Only:
                //    // Force OAuth2 without impersonation
                //    credential = await _googleCredentials.GetUserGoogleCredentialViaOAuthAsync(new[] { CalendarService.Scope.Calendar });
                //    break;

                default:
                    throw new ArgumentException($"Unsupported authentication mode: {authMode}");
            }

            return new CalendarService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });
        }

        private string GetCurrentUserEmail()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // Try to get email from various claim types in order of preference
            var emailClaim = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email) ??
                             httpContext?.User?.FindFirst("email") ??
                             httpContext?.User?.FindFirst("preferred_username") ??
                             httpContext?.User?.FindFirst("upn") ??
                             httpContext?.User?.FindFirst("unique_name");

            return emailClaim?.Value;
        }

        private string GetUserEmailForAuthMode(CalendarAuthMode authMode, string userEmail)
        {
            // For organization calendar, we don't need user email
            if (authMode == CalendarAuthMode.OrganizationCalendar)
            {
                return null;
            }

            // For user calendar, use provided email or get current user's email
            return userEmail ?? GetCurrentUserEmail();
        }

        private string GetApplicationName()
        {
            var settings = _settingRepository.GetSettings("Site");
            var serviceKey = settings.FirstOrDefault(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey");

            if (serviceKey != null && !string.IsNullOrEmpty(serviceKey.SettingValue))
            {
                try
                {
                    var creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);
                    return creds.project_id ?? "Oqtane Google Calendar Module";
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Other, "Failed to parse service account for app name: {Error}", ex.Message);
                }
            }

            return "Oqtane Google Calendar Module";
        }
    }
}