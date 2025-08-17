using System;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using Google.Apis.Calendar.v3.Data;
using Dev1.Module.GoogleAdmin.Models;
using Dev1.Module.GoogleAdmin.Shared.Models;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class GoogleCalendarService : ServiceBase, IGoogleCalendarService
    {
        public GoogleCalendarService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("GoogleCalendar");

        public async Task<CalendarAuthInfo> GetCalendarAuthInfoAsync(int moduleId, string userEmail)
        {
            var url = $"{ApiUrl}/authinfo?moduleid={moduleId}";
            if (!string.IsNullOrEmpty(userEmail))
            {
                url += $"&useremail={Uri.EscapeDataString(userEmail)}";
            }
            return await GetJsonAsync<CalendarAuthInfo>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<CalendarList> GetAvailableGoogleCalendarsAsync(int moduleId, CalendarAuthMode authMode, string userEmail)
        {
            var url = $"{ApiUrl}/calendars?moduleid={moduleId}&authmode={authMode}";
            if (!string.IsNullOrEmpty(userEmail))
            {
                url += $"&useremail={Uri.EscapeDataString(userEmail)}";
            }
            return await GetJsonAsync<CalendarList>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<Google.Apis.Calendar.v3.Data.Calendar> GetGoogleCalendarAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string userEmail)
        {
            var url = $"{ApiUrl}/calendar?moduleid={moduleId}&calendarid={Uri.EscapeDataString(calendarId)}&authmode={authMode}";
            if (!string.IsNullOrEmpty(userEmail))
            {
                url += $"&useremail={Uri.EscapeDataString(userEmail)}";
            }
            return await GetJsonAsync<Google.Apis.Calendar.v3.Data.Calendar>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<Events> GetCalendarEventsAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string userEmail)
        {
            var url = $"{ApiUrl}/events?moduleid={moduleId}&calendarid={Uri.EscapeDataString(calendarId)}&authmode={authMode}";
            if (!string.IsNullOrEmpty(userEmail))
            {
                url += $"&useremail={Uri.EscapeDataString(userEmail)}";
            }
            return await GetJsonAsync<Events>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<string> ScheduleCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode,
            string timezone, DateTime startDate, DateTime endDate, string summary, 
            string attendeeName, string attendeeEmail, string userEmail)
        {
            var eventRequest = new CreateEventRequest
            {
                ModuleId = moduleId,
                CalendarId = calendarId,
                AuthMode = authMode,
                Timezone = timezone,
                StartDate = startDate,
                EndDate = endDate,
                Summary = summary,
                AttendeeName = attendeeName,
                AttendeeEmail = attendeeEmail,
                UserEmail = userEmail
            };

            var result = await PostJsonAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/events", EntityNames.Module, moduleId), eventRequest);
            return result?.ToString() ?? string.Empty;
        }

        public async Task<string> CreateExtendedCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, ExtendedAppointment appointment, string userEmail)
        {
            var eventRequest = new ExtendedEventRequest
            {
                ModuleId = moduleId,
                CalendarId = calendarId,
                AuthMode = authMode,
                Appointment = appointment,
                UserEmail = userEmail
            };

            var result = await PostJsonAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/events/extended", EntityNames.Module, moduleId), eventRequest);
            return result?.ToString() ?? string.Empty;
        }

        public async Task<string> UpdateCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, ExtendedAppointment appointment, string userEmail)
        {
            var eventRequest = new ExtendedEventRequest
            {
                ModuleId = moduleId,
                CalendarId = calendarId,
                AuthMode = authMode,
                Appointment = appointment,
                UserEmail = userEmail
            };

            var result = await PutJsonAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/events/extended", EntityNames.Module, moduleId), eventRequest);
            return result?.ToString() ?? string.Empty;
        }

        public async Task DeleteCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string eventId, string userEmail)
        {
            var url = $"{ApiUrl}/events?moduleid={moduleId}&calendarid={Uri.EscapeDataString(calendarId)}&authmode={authMode}&eventid={Uri.EscapeDataString(eventId)}";
            if (!string.IsNullOrEmpty(userEmail))
            {
                url += $"&useremail={Uri.EscapeDataString(userEmail)}";
            }
            await DeleteAsync(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<string> GetCalendarAccessLevelAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string userEmail)
        {
            var url = $"{ApiUrl}/access?moduleid={moduleId}&calendarid={Uri.EscapeDataString(calendarId)}&authmode={authMode}";
            if (!string.IsNullOrEmpty(userEmail))
            {
                url += $"&useremail={Uri.EscapeDataString(userEmail)}";
            }
            return await GetJsonAsync<string>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        // Legacy methods for backward compatibility
        public async Task<CalendarList> GetAvailableGoogleCalendarsAsync(int moduleId, string impersonateAccount)
        {
            var authMode = string.IsNullOrEmpty(impersonateAccount) ? CalendarAuthMode.OrganizationCalendar : CalendarAuthMode.UserCalendar;
            return await GetAvailableGoogleCalendarsAsync(moduleId, authMode, impersonateAccount);
        }

        public async Task<string> ScheduleCalendarEventAsync(int moduleId, string impersonateAccount, string calendarId, 
            string timezone, DateTime startDate, DateTime endDate, string summary, 
            string attendeeName, string attendeeEmail)
        {
            var authMode = string.IsNullOrEmpty(impersonateAccount) ? CalendarAuthMode.OrganizationCalendar : CalendarAuthMode.UserCalendar;
            return await ScheduleCalendarEventAsync(moduleId, calendarId, authMode, timezone, startDate, endDate, summary, attendeeName, attendeeEmail, impersonateAccount);
        }

        public class CreateEventRequest
        {
            public int ModuleId { get; set; }
            public string CalendarId { get; set; }
            public CalendarAuthMode AuthMode { get; set; }
            public string Timezone { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Summary { get; set; }
            public string AttendeeName { get; set; }
            public string AttendeeEmail { get; set; }
            public string UserEmail { get; set; }
        }

        public class ExtendedEventRequest
        {
            public int ModuleId { get; set; }
            public string CalendarId { get; set; }
            public CalendarAuthMode AuthMode { get; set; }
            public ExtendedAppointment Appointment { get; set; }
            public string UserEmail { get; set; }
        }
    }
}
