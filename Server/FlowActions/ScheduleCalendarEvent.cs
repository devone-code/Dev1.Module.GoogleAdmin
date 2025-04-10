
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Dev1.Flow.Core;
using Dev1.Flow.Core.Models;
using Dev1.Module.GoogleAdmin.Services;
using Dev1.Module.Flow.Helpers;
using System;
using Oqtane.Repository;

//This class, along with the razor view for this Flow Action need the same namespace and Name
//For display purposes, the Razor view may contain spaces, flow will remove these when attempting to find the Flow Processor for this action
//EG:
//  Notification Email.razor and NotificationEmail.cs will work 
//  NotificationEmail.razor and NotificationEmai.cs will work
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
                var Description = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Description");


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
                    Description, user.DisplayName, user.Email);

                WorkflowItem.Status = (int)eActionStatus.Pass;

            }
            catch(Exception ex)
            {
                WorkflowItem.LastResponse = ex.Message;
                WorkflowItem.Status = (int)eActionStatus.Fail;
            }
        }
    }
}
