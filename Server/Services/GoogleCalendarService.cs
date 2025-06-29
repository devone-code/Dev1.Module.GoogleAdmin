using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;

using Dev1.Module.GoogleAdmin.Repository;


using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Auth.OAuth2;
using Oqtane.Repository;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;
using System;

namespace Dev1.Module.GoogleAdmin.Services
{


    public class ServerGoogleCalendarService : IGoogleCalendarService
    {

        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;
        private readonly IGoogleCredentials _googleCredentials;

        private readonly ISettingRepository _settingRepo;

        public ServerGoogleCalendarService(
            IUserPermissions userPermissions, ITenantManager tenantManager,
            ISettingRepository settingRepo,
            IGoogleCredentials googleCredentials,
            ILogManager logger, IHttpContextAccessor accessor)
        {

            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
            _settingRepo = settingRepo;
            _googleCredentials = googleCredentials;
        }


        public async Task<CalendarList> GetAvailableGoogleCalendarsAsync(int ModuleId, string impersonateAccount)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                CalendarService calendarService = InitializeCalendarService(impersonateAccount, new[] { CalendarService.Scope.Calendar });
                //string calendarName = null;



                //calendarName = settings.Any(x => x.SettingName == "Dev1.GoogleAdmin:_defaultCalendarId") ? settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_defaultCalendarId").FirstOrDefault().SettingValue : "";

                //}

                try
                {
                    var calendars = await calendarService.CalendarList.List().ExecuteAsync();

                    return calendars;

                    //var e = await calendarService.Events.List(calendar.Id).ExecuteAsync();
                }
                catch (Google.GoogleApiException e)
                {
                    throw new System.Exception(e.Message);
                }
                catch (Exception ex)
                {
                    throw;
                }

            }
            return null;
        }


        public async Task<Calendar> GetGoogleCalendarAsync(int ModuleId, string impersonateAccount)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                
                string calendarName = null;
                var settings = _settingRepo.GetSettings("Site");

                CalendarService calendarService = InitializeCalendarService(impersonateAccount, new[] { CalendarService.Scope.Calendar });

                calendarName = settings.Any(x => x.SettingName == "Dev1.GoogleAdmin:_defaultCalendarId") ? settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_defaultCalendarId").FirstOrDefault().SettingValue : "";

                try
                {
                    var calendar = await calendarService.Calendars.Get(calendarName).ExecuteAsync();

                    return calendar;

                    //var e = await calendarService.Events.List(calendar.Id).ExecuteAsync();
                }
                catch (Google.GoogleApiException e)
                {
                    throw (new System.Exception(e.Message));
                }

            }
            return null;

        }

        public async Task<Events> GetCalendarEventsAsync(int ModuleId, string CalendarId, string impersonateAccount)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                CalendarService calendarService = InitializeCalendarService(impersonateAccount, new[] { CalendarService.Scope.Calendar });

                try
                {
                    var events = await calendarService.Events.List(CalendarId).ExecuteAsync();

                    return events;

                    //var e = await calendarService.Events.List(calendar.Id).ExecuteAsync();
                }
                catch (Google.GoogleApiException e)
                {
                    throw (new System.Exception(e.Message));
                }

            }
            return null;

        }


        public async Task<string> ScheduleCalendarEventAsync(int ModuleId, string impersonateAccount, string CalendarId, string Timezone, DateTime StartDate, DateTime EndDate,
            string Summary, 
            string AttendeeName, string AttendeeEmail
            )
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {

                CalendarService calendarService = InitializeCalendarService(impersonateAccount, new[] { CalendarService.Scope.CalendarEvents });

                try
                {
                    var ev = new Event();
                    EventDateTime start = new EventDateTime();
                    
                    start.DateTimeDateTimeOffset = StartDate;
                    start.TimeZone = Timezone;

                    EventDateTime end = new EventDateTime();
                    end.DateTimeDateTimeOffset = EndDate;
                    end.TimeZone = Timezone;

                    ev.Start = start;
                    ev.End = end;
                    ev.Summary = Summary;
                    //ev.Description = Description;
                    ev.Attendees = new List<EventAttendee>();

                    ev.Attendees.Add(new EventAttendee()
                    {
                        DisplayName = AttendeeName,
                        Email = AttendeeEmail,

                    });


                    Event Event = await calendarService.Events.Insert(ev, CalendarId).ExecuteAsync();
                    Console.WriteLine("Event created: %s\n", Event.HtmlLink);
                    return Event.HtmlLink;
                    //var e = await calendarService.Events.List(calendar.Id).ExecuteAsync();
                }
                catch (Google.GoogleApiException e)
                {
                    throw (new System.Exception(e.Message));
                }

            }
            return null;

        }



        private CalendarService InitializeCalendarService(string impersonateAccount, string[] Scopes)
        {
            var settings = _settingRepo.GetSettings("Site");

            CalendarService calendarService;

            var serviceKey = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey").FirstOrDefault();

            if (serviceKey == null || String.IsNullOrEmpty(serviceKey.SettingValue)) {
                throw new Exception("The service key Setting value was not found.");
            
            }

            Shared.Models.AccountCredentials creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);

            if (String.IsNullOrEmpty(impersonateAccount))
            {
                var credential = _googleCredentials.GetServiceAccountCredentialFromServiceKey(Scopes);
                calendarService = new CalendarService(new Google.Apis.Services.BaseClientService.Initializer()
                {

                    HttpClientInitializer = credential,
                    ApplicationName = creds.project_id,

                });
            }
            else
            {
                var credential = _googleCredentials.GetGoogleCredentialFromServiceKey(Scopes, impersonateAccount);
                calendarService = new CalendarService(new Google.Apis.Services.BaseClientService.Initializer()
                {

                    HttpClientInitializer = credential,
                    ApplicationName = creds.project_id,

                });
            }

            return calendarService;

        }
    }

}
