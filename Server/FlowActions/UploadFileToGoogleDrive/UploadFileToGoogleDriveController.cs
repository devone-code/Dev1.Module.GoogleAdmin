using Dev1.Flow.Core;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Infrastructure;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Dev1.Module.GoogleAdmin.GoogleAction
{
    // Accepts external webhook calls to advance WebhookGate actions.
    [Route("api/[controller]")]
    [FlowWebhookEndpoint(typeof(UploadFileToGoogleDrive))]
    public class UploadFileToGoogleDriveController : FlowWebhookControllerBase<UploadFileToGoogleDrive>
    {
        public UploadFileToGoogleDriveController(IWorkflowWebhookGateway gateway, ILogManager logger)
            : base(gateway, logger) { }

        // Optional override to parse email or perform additional checks if needed,
        //otherwise remove and let the base class handle this.
        protected override Task<string> OnReceivedAsync(WebhookRequestContext ctx)
        {
            //When a webhook comes in, the system will attempt to find any 
            //workflows whose next action is your Custom Action (in this instance
            //it will try to find WebhookGate actions). Unless you set an email
            //address in the WebhookRequestContext, the system will find all workflows
            //that meet these rules.
            //This may by what you require, but if you only want to process awaiting
            //actions for a specific user, you MUST set the WebhookRequestContext
            //email address to the address contained in the POST payload.
            //For instance, you may have a Workflow this belongs to admin@work.com
            //(This is set when the workflow is created and stored in the Workflow.ContextEmail
            //field).
            //You have a cool Custom Flow Action that adds a user to an Oqtane Role
            //after they join your Slack Channel.
            //For this, you create an outgoing webhook in Slack which points to your
            //Action WebhookController. You then use the property email name (from Slack)
            //to add the email address to the WebhookRequestContext.
            //You might even be required to call the external system to retrieve the email,
            //you will need to check the system's API documentation for this.
            if (ctx.Flat != null && ctx.Flat.TryGetValue("email", out var email) && !string.IsNullOrWhiteSpace(email))
            {
                ctx.Email = email;
            }
            return Task.FromResult<string>(null);
        }
    }
}
