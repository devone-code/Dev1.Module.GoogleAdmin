using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Dev1.Flow.Core;
using Dev1.Flow.Core.Models;
using Dev1.Module.GoogleAdmin.Services;
using Dev1.Module.Flow.Helpers;
using System;
using Oqtane.Repository;
using System.Collections.Generic;
using NodaTime.TimeZones;
using System.Linq;

//This class, along with the razor view for this Flow Action need the same namespace and Name
//For display purposes, the Razor view may contain spaces, flow will remove these when attempting to find the Flow Processor for this action
//EG:
//  Notification Email.razor and NotificationEmail.cs will work 
//  Notification Email.razor and NotificationEmai.cs will work
//
//  Notification EmailView.razor and NotificationEmail.cs will not work as they have different names once the spaces have been removed

namespace Dev1.Module.GoogleAdmin.GoogleAction
{
    [FlowProcessor(serviceLifetime: ServiceLifetime.Scoped)]
    public class ScheduleCalendarEvent : IFlowProcessor
    {
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IUserRepository _userRepository;

        public ScheduleCalendarEvent(IGoogleCalendarService googleCalendarService,IUserRepository userRepository)
        {
            _googleCalendarService = googleCalendarService;
            _userRepository = userRepository;
        }

        public List<ActionPropertyDefinition> PropertyDefinitions => new List<ActionPropertyDefinition>
        {
                 new ActionPropertyDefinition
                {
                    Name = "Organisation Calendar",
                    InputTypeId = Convert.ToInt16(eDataType.Bool),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = false
                 },


            new ActionPropertyDefinition
                {
                    Name = "Calendar",
                    InputTypeId = Convert.ToInt16(eDataType.String),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = true
                },


            new ActionPropertyDefinition
                {
                    Name = "Timezone",
                    InputTypeId = Convert.ToInt16(eDataType.String),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = true
                },

            new ActionPropertyDefinition
                {
                    Name = "Start Date",
                    InputTypeId = Convert.ToInt16(eDataType.Date),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = true
                },

            new ActionPropertyDefinition
                {
                    Name = "End Date",
                    InputTypeId = Convert.ToInt16(eDataType.Date),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = true
                },

            new ActionPropertyDefinition
                {
                    Name = "Summary",
                    InputTypeId = Convert.ToInt16(eDataType.String),
                    ForceWorkflow = false,
                    IsForWorkflow = false,
                    IsRequired = true
                }
        };

        public async Task ExecuteActionAsync(Workflow Workflow, WorkflowItem WorkflowItem, int SiteId)
        {
            try
            {



                ////Get the properties we need to process this item.
                var OrganisationCalendar = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Organisation Calendar");
                var Calendar = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Calendar");
                var Timezone = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Timezone");
                var StartDate = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Start Date");
                var EndDate = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "End Date");
                var Summary = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Summary");
                //var Description = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Description");


                //This action should have an additional property of "User to Add":
                //1 SourceUser: The user who was logged in when this flow was triggered
                //2 Specific User: Email address
                //For now, just use the sigend in user (Workflow.CreatedBy).

                var user = _userRepository.GetUser(WorkflowItem.ProcessedByUserId);
                bool ForOrganisation;
                Boolean.TryParse(OrganisationCalendar, out ForOrganisation);
                string impersonateAccount = null;

                if (!ForOrganisation)
                    impersonateAccount = user.Email;

                await _googleCalendarService.ScheduleCalendarEventAsync(Workflow.Flow.ModuleId, impersonateAccount, 
                    Calendar, Timezone, Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), Summary,
                    user.DisplayName, user.Email);

                WorkflowItem.Status = (int)eActionStatus.Pass;

            }
            catch(Exception ex)
            {
                WorkflowItem.LastResponse = ex.Message;
                WorkflowItem.Status = (int)eActionStatus.Fail;
            }
        }

        public async Task<ActionDataResponse> GetActionDataAsync(string propertyName, Workflow workflow, WorkflowItem workflowItem, int siteId)
        {
            try
            {
                switch (propertyName)
                {
                    case "Calendar":
                        var calendars = await _googleCalendarService.GetAvailableGoogleCalendarsAsync(workflow.ModuleId, workflow.CreatedBy);
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

                    case "Organisation Calendar":
                        return new ActionDataResponse
                        {
                            Success = true,
                            Items = new List<ActionDataItem> 
                            { 
                                new ActionDataItem { Value = "true", Text = "Yes" },
                                new ActionDataItem { Value = "false", Text = "No" }
                            }
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
