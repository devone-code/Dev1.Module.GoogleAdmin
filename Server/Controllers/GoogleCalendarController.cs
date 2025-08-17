using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Controllers;
using Dev1.Module.GoogleAdmin.Services;
using Dev1.Module.GoogleAdmin.Models;
using Google.Apis.Calendar.v3.Data;
using System.Threading.Tasks;
using System.Net;
using System;
using Dev1.Module.GoogleAdmin.Shared.Models;

namespace Dev1.Module.GoogleAdmin.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class GoogleCalendarController : ModuleControllerBase
    {
        private readonly IGoogleCalendarService _googleCalendarService;

        public GoogleCalendarController(IGoogleCalendarService googleCalendarService, ILogManager logger, IHttpContextAccessor accessor) 
            : base(logger, accessor)
        {
            _googleCalendarService = googleCalendarService;
        }

        // GET: api/GoogleCalendar/authinfo?moduleid=x&useremail=x
        [HttpGet("authinfo")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<CalendarAuthInfo> GetAuthInfo(int moduleId, string userEmail)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                return await _googleCalendarService.GetCalendarAuthInfoAsync(moduleId, userEmail);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Calendar Auth Info Get Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new CalendarAuthInfo { ErrorMessage = "Unauthorized access" };
            }
        }

        // GET: api/GoogleCalendar/calendars?moduleid=x&authmode=x&useremail=x
        [HttpGet("calendars")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<CalendarList> GetCalendars(int moduleId, CalendarAuthMode authMode, string userEmail)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                try
                {
                    return await _googleCalendarService.GetAvailableGoogleCalendarsAsync(moduleId, authMode, userEmail);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "Error getting calendars for module {ModuleId} user {UserEmail}: {Error}", moduleId, userEmail ?? "current", ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Calendar Get Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET: api/GoogleCalendar/calendar?moduleid=x&calendarid=x&authmode=x&useremail=x
        [HttpGet("calendar")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Calendar> GetCalendar(int moduleId, string calendarId, CalendarAuthMode authMode, string userEmail)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                try
                {
                    return await _googleCalendarService.GetGoogleCalendarAsync(moduleId, calendarId, authMode, userEmail);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "Error getting calendar {CalendarId} for module {ModuleId} user {UserEmail}: {Error}", calendarId, moduleId, userEmail ?? "current", ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Calendar Get Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET: api/GoogleCalendar/events?moduleid=x&calendarid=x&authmode=x&useremail=x
        [HttpGet("events")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Events> GetEvents(int moduleId, string calendarId, CalendarAuthMode authMode, string userEmail)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                try
                {
                    return await _googleCalendarService.GetCalendarEventsAsync(moduleId, calendarId, authMode, userEmail);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "Error getting events for calendar {CalendarId} module {ModuleId} user {UserEmail}: {Error}", calendarId, moduleId, userEmail ?? "current", ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Calendar Events Get Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST: api/GoogleCalendar/events
        [HttpPost("events")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<string> CreateEvent([FromBody] CreateEventRequest request)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, request.ModuleId))
            {
                try
                {
                    return await _googleCalendarService.ScheduleCalendarEventAsync(
                        request.ModuleId, 
                        request.CalendarId, 
                        request.AuthMode,
                        request.Timezone, 
                        request.StartDate, 
                        request.EndDate, 
                        request.Summary,
                        request.AttendeeName, 
                        request.AttendeeEmail,
                        request.UserEmail);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "Error creating calendar event for module {ModuleId} user {UserEmail}: {Error}", request.ModuleId, request.UserEmail ?? "current", ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Calendar Event Create Attempt {ModuleId}", request.ModuleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST: api/GoogleCalendar/events/extended
        [HttpPost("events/extended")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<string> CreateExtendedEvent([FromBody] ExtendedEventRequest request)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, request.ModuleId))
            {
                try
                {
                    return await _googleCalendarService.CreateExtendedCalendarEventAsync(request.ModuleId, request.CalendarId, request.AuthMode, request.Appointment, request.UserEmail);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "Error creating extended event for module {ModuleId} user {UserEmail}: {Error}", request.ModuleId, request.UserEmail ?? "current", ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Extended Event Create Attempt {ModuleId}", request.ModuleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // PUT: api/GoogleCalendar/events/extended
        [HttpPut("events/extended")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<string> UpdateExtendedEvent([FromBody] ExtendedEventRequest request)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, request.ModuleId))
            {
                try
                {
                    return await _googleCalendarService.UpdateCalendarEventAsync(request.ModuleId, request.CalendarId, request.AuthMode, request.Appointment, request.UserEmail);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Update, "Error updating extended event for module {ModuleId} user {UserEmail}: {Error}", request.ModuleId, request.UserEmail ?? "current", ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Extended Event Update Attempt {ModuleId}", request.ModuleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // DELETE: api/GoogleCalendar/events?moduleid=x&calendarid=x&authmode=x&eventid=x&useremail=x
        [HttpDelete("events")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task DeleteEvent(int moduleId, string calendarId, CalendarAuthMode authMode, string eventId, string userEmail)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                try
                {
                    await _googleCalendarService.DeleteCalendarEventAsync(moduleId, calendarId, authMode, eventId, userEmail);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Delete, "Error deleting event {EventId} for module {ModuleId} user {UserEmail}: {Error}", eventId, moduleId, userEmail ?? "current", ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Event Delete Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // GET: api/GoogleCalendar/access?moduleid=x&calendarid=x&authmode=x&useremail=x
        [HttpGet("access")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<string> GetCalendarAccess(int moduleId, string calendarId, CalendarAuthMode authMode, string userEmail)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                try
                {
                    return await _googleCalendarService.GetCalendarAccessLevelAsync(moduleId, calendarId, authMode, userEmail);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "Error getting calendar access for {CalendarId} module {ModuleId} user {UserEmail}: {Error}", calendarId, moduleId, userEmail ?? "current", ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "none";
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Calendar Access Check Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return "none";
            }
        }

        // Backwards compatibility endpoints
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<CalendarList> Get(int moduleId, string impersonateAccount = null)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                try
                {
                    var authMode = string.IsNullOrEmpty(impersonateAccount) ? CalendarAuthMode.OrganizationCalendar : CalendarAuthMode.UserCalendar;
                    return await _googleCalendarService.GetAvailableGoogleCalendarsAsync(moduleId, authMode, impersonateAccount);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "Error getting calendars for module {ModuleId}: {Error}", moduleId, ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Calendar Get Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
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
