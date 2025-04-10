
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Dev1.Flow.Core;
using Dev1.Flow.Core.Models;
using Dev1.Module.GoogleAdmin.Services;
using Dev1.Module.Flow.Helpers;
using System;


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
    public class AddUserToGoogleGroup : IFlowProcessor
    {
        private readonly IGoogleDirectoryService _googleDirectoryService;


        public AddUserToGoogleGroup(IGoogleDirectoryService googleDirectoryService)
        {
            _googleDirectoryService = googleDirectoryService;
        }
        public async Task ExecuteActionAsync(Workflow Workflow, WorkflowItem WorkflowItem, int SiteId)
        {
            try
            {



                ////Get the properties we need to process this item.
                var group = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "User Group");
                var role = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Role");




                //This action should have an additional property of "User to Add":
                //1 SourceUser: The user who was logged in when this flow was triggered
                //2 Specific User: Email address
                //For now, just use the sigend in user (Workflow.CreatedBy).
                await _googleDirectoryService.AddMemberToGroup(group,Workflow.CreatedBy,role,Workflow.ModuleId);

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
