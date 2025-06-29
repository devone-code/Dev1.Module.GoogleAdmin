using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Services
{
    public interface IGoogleCalendarService
    {
        Task<CalendarList> GetAvailableGoogleCalendarsAsync(int ModuleId, string impersonateAccount);
        Task<Calendar> GetGoogleCalendarAsync(int ModuleId, string impersonateAccount);
        Task<Events> GetCalendarEventsAsync(int ModuleId, string calendarId, string impersonateAccount);
        Task<string> ScheduleCalendarEventAsync(int ModuleId, string impersonateAccount, string CalendarId, string Timezone, DateTime StartDate, DateTime EndDate,
                    string Summary, 
                    string AttendeeName, string AttendeeEmail);

    }
}
