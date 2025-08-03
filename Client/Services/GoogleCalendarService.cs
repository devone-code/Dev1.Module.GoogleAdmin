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

        public async Task<CalendarAuthInfo> GetCalendarAuthInfoAsync(int moduleId)
        {
            return await GetJsonAsync<CalendarAuthInfo>(CreateAuthorizationPolicyUrl($"{ApiUrl}/authinfo?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<CalendarList> GetAvailableGoogleCalendarsAsync(int moduleId, CalendarAuthMode authMode)
        {
            return await GetJsonAsync<CalendarList>(CreateAuthorizationPolicyUrl($"{ApiUrl}/calendars?moduleid={moduleId}&authmode={authMode}", EntityNames.Module, moduleId));
        }

        public async Task<Google.Apis.Calendar.v3.Data.Calendar> GetGoogleCalendarAsync(int moduleId, string calendarId, CalendarAuthMode authMode)
        {
            return await GetJsonAsync<Google.Apis.Calendar.v3.Data.Calendar>(CreateAuthorizationPolicyUrl($"{ApiUrl}/calendar?moduleid={moduleId}&calendarid={Uri.EscapeDataString(calendarId)}&authmode={authMode}", EntityNames.Module, moduleId));
        }

        public async Task<Events> GetCalendarEventsAsync(int moduleId, string calendarId, CalendarAuthMode authMode)
        {
            return await GetJsonAsync<Events>(CreateAuthorizationPolicyUrl($"{ApiUrl}/events?moduleid={moduleId}&calendarid={Uri.EscapeDataString(calendarId)}&authmode={authMode}", EntityNames.Module, moduleId));
        }

        public async Task<string> ScheduleCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode,
            string timezone, DateTime startDate, DateTime endDate, string summary, 
            string attendeeName, string attendeeEmail)
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
                AttendeeEmail = attendeeEmail
            };

            var result = await PostJsonAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/events", EntityNames.Module, moduleId), eventRequest);
            return result?.ToString() ?? string.Empty;
        }

        public async Task<string> CreateExtendedCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, ExtendedAppointment appointment)
        {
            var eventRequest = new ExtendedEventRequest
            {
                ModuleId = moduleId,
                CalendarId = calendarId,
                AuthMode = authMode,
                Appointment = appointment
            };

            var result = await PostJsonAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/events/extended", EntityNames.Module, moduleId), eventRequest);
            return result?.ToString() ?? string.Empty;
        }

        public async Task<string> UpdateCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, ExtendedAppointment appointment)
        {
            var eventRequest = new ExtendedEventRequest
            {
                ModuleId = moduleId,
                CalendarId = calendarId,
                AuthMode = authMode,
                Appointment = appointment
            };

            var result = await PutJsonAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/events/extended", EntityNames.Module, moduleId), eventRequest);
            return result?.ToString() ?? string.Empty;
        }

        public async Task DeleteCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string eventId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/events?moduleid={moduleId}&calendarid={Uri.EscapeDataString(calendarId)}&authmode={authMode}&eventid={Uri.EscapeDataString(eventId)}", EntityNames.Module, moduleId));
        }

        // Methods still needed by existing code
        public async Task<CalendarList> GetAvailableGoogleCalendarsAsync(int moduleId, string impersonateAccount)
        {
            var authMode = string.IsNullOrEmpty(impersonateAccount) ? CalendarAuthMode.OrganizationCalendar : CalendarAuthMode.UserCalendar;
            return await GetAvailableGoogleCalendarsAsync(moduleId, authMode);
        }

        public async Task<string> ScheduleCalendarEventAsync(int moduleId, string impersonateAccount, string calendarId, 
            string timezone, DateTime startDate, DateTime endDate, string summary, 
            string attendeeName, string attendeeEmail)
        {
            var authMode = string.IsNullOrEmpty(impersonateAccount) ? CalendarAuthMode.OrganizationCalendar : CalendarAuthMode.UserCalendar;
            return await ScheduleCalendarEventAsync(moduleId, calendarId, authMode, timezone, startDate, endDate, summary, attendeeName, attendeeEmail);
        }

        public async Task<string> GetCalendarAccessLevelAsync(int moduleId, string calendarId, CalendarAuthMode authMode)
        {
            return await GetJsonAsync<string>(CreateAuthorizationPolicyUrl($"{ApiUrl}/access?moduleid={moduleId}&calendarid={Uri.EscapeDataString(calendarId)}&authmode={authMode}", EntityNames.Module, moduleId));
        }

        public class ExtendedEventRequest
        {
            public int ModuleId { get; set; }
            public string CalendarId { get; set; }
            public CalendarAuthMode AuthMode { get; set; }
            public ExtendedAppointment Appointment { get; set; }
        }
    }
}
