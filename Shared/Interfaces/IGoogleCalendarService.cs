using Google.Apis.Calendar.v3.Data;
using Dev1.Module.GoogleAdmin.Models;
using Dev1.Module.GoogleAdmin.Shared.Models;
using System;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Services
{
    public interface IGoogleCalendarService
    {
        /// <summary>
        /// Gets information about available authentication methods
        /// </summary>
        Task<CalendarAuthInfo> GetCalendarAuthInfoAsync(int moduleId);
        
        /// <summary>
        /// Gets available Google calendars based on authentication mode
        /// </summary>
        Task<CalendarList> GetAvailableGoogleCalendarsAsync(int moduleId, CalendarAuthMode authMode);
        
        /// <summary>
        /// Gets a specific Google calendar
        /// </summary>
        Task<Calendar> GetGoogleCalendarAsync(int moduleId, string calendarId, CalendarAuthMode authMode);
        
        /// <summary>
        /// Gets events from a specific calendar
        /// </summary>
        Task<Events> GetCalendarEventsAsync(int moduleId, string calendarId, CalendarAuthMode authMode);
        
        /// <summary>
        /// Schedules a new calendar event
        /// </summary>
        Task<string> ScheduleCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode,
            string timezone, DateTime startDate, DateTime endDate, string summary, 
            string attendeeName, string attendeeEmail);

        /// <summary>
        /// Creates a new calendar event with extended properties
        /// </summary>
        Task<string> CreateExtendedCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, ExtendedAppointment appointment);

        /// <summary>
        /// Updates an existing calendar event
        /// </summary>
        Task<string> UpdateCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, ExtendedAppointment appointment);

        /// <summary>
        /// Deletes a calendar event
        /// </summary>
        Task DeleteCalendarEventAsync(int moduleId, string calendarId, CalendarAuthMode authMode, string eventId);

        /// <summary>
        /// Gets the current user's access level to a specific calendar
        /// </summary>
        Task<string> GetCalendarAccessLevelAsync(int moduleId, string calendarId, CalendarAuthMode authMode);

        // Methods still needed by existing code
        Task<CalendarList> GetAvailableGoogleCalendarsAsync(int moduleId, string impersonateAccount);
        Task<string> ScheduleCalendarEventAsync(int moduleId, string impersonateAccount, string calendarId, 
            string timezone, DateTime startDate, DateTime endDate, string summary, 
            string attendeeName, string attendeeEmail);
    }
}
