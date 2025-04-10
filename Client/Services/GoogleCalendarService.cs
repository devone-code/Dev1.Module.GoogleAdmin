using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class GoogleCalendarService : ServiceBase, IGoogleCalendarService
    {
        public GoogleCalendarService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("GoogleCalendar");


        public async Task<Google.Apis.Calendar.v3.Data.CalendarList> GetAvailableGoogleCalendarsAsync(int ModuleId, string impersonateAccount)
        {
            var calendars = await GetJsonAsync<Google.Apis.Calendar.v3.Data.CalendarList>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), null);
            return calendars;
        }

        public async Task<Google.Apis.Calendar.v3.Data.Calendar> GetGoogleCalendarAsync(int ModuleId, string impersonateAccount)
        {
            var calendar = await GetJsonAsync<Google.Apis.Calendar.v3.Data.Calendar>(CreateAuthorizationPolicyUrl($"{Apiurl}?ImpersonateAccount={impersonateAccount}&moduleid={ModuleId}", EntityNames.Module, ModuleId), null);
            return calendar;
        }

        public async Task<Google.Apis.Calendar.v3.Data.Events> GetCalendarEventsAsync(int ModuleId, string CalendarId, string impersonateAccount)
        {
            var events = await GetJsonAsync<Google.Apis.Calendar.v3.Data.Events>(CreateAuthorizationPolicyUrl($"{Apiurl}/events?ImpersonateAccount={impersonateAccount}&CalendarId={CalendarId}&moduleid={ModuleId}", EntityNames.Module, ModuleId), null);
            return events;
        }

        public async Task<string> ScheduleCalendarEventAsync(int ModuleId, string impersonateAccount, string CalendarId, string Timezone, DateTime StartDate, DateTime EndDate,
                            string Summary, string Description,
                            string AttendeeName, string AttendeeEmail)
        { 

        var eventlink = await GetJsonAsync<string>(CreateAuthorizationPolicyUrl($"{Apiurl}/events?ImpersonateAccount={impersonateAccount}&moduleid={ModuleId}&CalendarId={CalendarId}&StartDate={StartDate}&EndDate={EndDate}&Summary={Summary}&Description={Description}&AttendeeName={AttendeeName}&AttendeeEmail={AttendeeEmail}", EntityNames.Module, ModuleId), null);
            return eventlink;
            }


    }
}
