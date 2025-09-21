using Dev1.Flow.Core;
using Dev1.Flow.Core.DTOs;
using Dev1.Flow.Core.Helpers;
using Dev1.Flow.Core.Models;

using Dev1.Module.GoogleAdmin.Services;
using Dev1.Module.GoogleAdmin.Shared.Models;
using Oqtane.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.GoogleAction
{
    public class ScheduleCalendarEvent : IFlowProcessor
    {
        public string ActionName => "Schedule Google Calendar Event";
        public string ActionDescription => "Schedule a calendar event in Google (requires the user processing this item to be logged in via Google).";
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IUserRepository _userRepository;

        public List<string> ContextRequirements => new List<string>();

        public ScheduleCalendarEvent(IGoogleCalendarService googleCalendarService,IUserRepository userRepository)
        {
            _googleCalendarService = googleCalendarService;
            _userRepository = userRepository;
        }

        public List<ActionPropertyDefinition> PropertyDefinitions => new List<ActionPropertyDefinition>
        {
                // new ActionPropertyDefinition
                //{
                //    Name = "Organisation Calendar",
                //    InputTypeId = Convert.ToInt16(eInputType.Checkbox),
                //    ForceWorkflow = true,
                //    IsForWorkflow = true,
                //    IsRequired = false
                // },


            new ActionPropertyDefinition
                {
                    Name = "Calendar",
                    InputTypeId = Convert.ToInt16(eInputType.List),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = true
                },


            new ActionPropertyDefinition
                {
                    Name = "Timezone",
                    InputTypeId = Convert.ToInt16(eInputType.List),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = true
                },

            new ActionPropertyDefinition
                {
                    Name = "Start Date",
                    InputTypeId = Convert.ToInt16(eInputType.Date),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = true
                },

            new ActionPropertyDefinition
                {
                    Name = "End Date",
                    InputTypeId = Convert.ToInt16(eInputType.Date),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = true
                },

            new ActionPropertyDefinition
                {
                    Name = "Summary",
                    InputTypeId = Convert.ToInt16(eInputType.Text),
                    ForceWorkflow = false,
                    IsForWorkflow = false,
                    IsRequired = true
                },

            new ActionPropertyDefinition
                {
                    Name = "Location",
                    InputTypeId = Convert.ToInt16(eInputType.Text),
                    ForceWorkflow = false,
                    IsForWorkflow = false,
                    IsRequired = true
                }
        };

        public async Task<ExecuteActionContext> ExecuteActionAsync(ExecuteActionContext ActionContext)
        {
            try
            {
                ////Get the properties we need to process this item.
                var Calendar = FlowActionHelpers.GetItemPropertyValue(ActionContext.WorkflowItem, "Calendar");
                var Timezone = FlowActionHelpers.GetItemPropertyValue(ActionContext.WorkflowItem, "Timezone");
                var StartDate = FlowActionHelpers.GetItemPropertyValue(ActionContext.WorkflowItem, "Start Date");
                var EndDate = FlowActionHelpers.GetItemPropertyValue(ActionContext.WorkflowItem, "End Date");
                var Summary = FlowActionHelpers.GetItemPropertyValue(ActionContext.WorkflowItem, "Summary");
                var Location = FlowActionHelpers.GetItemPropertyValue(ActionContext.WorkflowItem, "Location");

                var user = _userRepository.GetUser(ActionContext.WorkflowItem.ProcessedByUserId);

                ExtendedAppointment appointment = new ExtendedAppointment()
                {
                    Timezone = Timezone,
                    AttendeeEmails = new List<string> { user.Email },
                    Description = Summary,
                    Start = new DateTime(Convert.ToInt64(StartDate)),
                    End = new DateTime(Convert.ToInt64(EndDate)),
                    Text = Summary,
                    Location = Location,
                };

                var eventId = await _googleCalendarService.CreateExtendedCalendarEventAsync(
                    ActionContext.ModuleId,
                    Calendar,
                    Shared.Models.CalendarAuthMode.UserCalendar,
                    appointment, ActionContext.ContextEmail
                );

                ActionContext.WorkflowItem.LastResponse = $"Google Event: {eventId} Created";
                ActionContext.WorkflowItem.Status = (int)eActionStatus.Pass;

            }
            catch(Exception ex)
            {
                ActionContext.WorkflowItem.LastResponse = ex.Message;
                ActionContext.WorkflowItem.Status = (int)eActionStatus.Fail;
            }
            return ActionContext;
        }

        public async Task<ActionDataResponse> GetActionDataAsync(string propertyName, int moduleid, int userid, int siteId)
        {
            try
            {
                switch (propertyName)
                {
                    case "Calendar":
                        var user = _userRepository.GetUser(userid);
                        var calendars = await _googleCalendarService.GetAvailableGoogleCalendarsAsync(moduleid, user.Email);
                        if (calendars?.Items != null)
                        {
                            return new ActionDataResponse
                            {
                                Success = true,
                                Items = calendars.Items.Select(c => new ActionDataItem 
                                { 
                                    Value = c.Id, 
                                    Text = c.Summary 
                                }).ToList()
                            };
                        }
                        break;

                    case "Timezone":
                        // Return Australian timezones as shown in the Razor page
                        var australianTimezones = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default.ZoneLocations
                            .Where(x => x.CountryCode == "AU")
                            .Select(x => x.ZoneId)
                            .ToList();
                        
                        return new ActionDataResponse
                        {
                            Success = true,
                            Items = australianTimezones.Select(tz => new ActionDataItem 
                            { 
                                Value = tz, 
                                Text = tz 
                            }).ToList()
                        };
                }

                return new ActionDataResponse
                {
                    Success = false,
                    ErrorMessage = $"Property '{propertyName}' not found or not supported for data retrieval"
                };
            }
            catch (Exception ex)
            {
                return new ActionDataResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }


    }
}
