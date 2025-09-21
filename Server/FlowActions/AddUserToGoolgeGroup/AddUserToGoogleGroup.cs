using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Dev1.Flow.Core;
using Dev1.Flow.Core.Models;
using Dev1.Module.GoogleAdmin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev1.Flow.Core.DTOs;
using Oqtane.Repository;
using Dev1.Flow.Core.Helpers;


namespace Dev1.Module.GoogleAdmin.GoogleAction
{
    public class AddUserToGoogleGroup : IFlowProcessor
    {
        public string ActionName => "Add User to Google Group";
        public string ActionDescription => "Adds a user to the specified Google Group. For manual processing, requires the user to be logged in via Google. For automated processing, uses service account credentials.";

        public List<string> ContextRequirements => new List<string>();

        private readonly IGoogleDirectoryService _googleDirectoryService;
        private readonly IUserRepository _userRepository;

        public AddUserToGoogleGroup(IGoogleDirectoryService googleDirectoryService, IUserRepository userRepository)
        {
            _googleDirectoryService = googleDirectoryService;
            _userRepository = userRepository;
        }

        public List<ActionPropertyDefinition> PropertyDefinitions => new List<ActionPropertyDefinition>
        {
            new ActionPropertyDefinition
            {
                    Name = "User Group",
                    InputTypeId = Convert.ToInt16(eInputType.List),
                    ForceWorkflow = false,
                    IsForWorkflow = false,
                    IsRequired = true
                },

             new ActionPropertyDefinition

            {
                Name = "Role",
                InputTypeId = Convert.ToInt16(eInputType.List),
                ForceWorkflow = false,
                IsForWorkflow = false,
                IsRequired = true
            }
        };

        public async Task<ExecuteActionContext> ExecuteActionAsync(ExecuteActionContext ActionContext)
        {
            try
            {

                // Get the properties we need to process this item
                var group = FlowActionHelpers.GetItemPropertyValue(ActionContext.WorkflowItem, "User Group");
                var role = FlowActionHelpers.GetItemPropertyValue(ActionContext.WorkflowItem, "Role");

                // Get the user to add to the group
                //var user = _userRepository.GetUser(userId);
                if (ActionContext.ContextEmail == null)
                {
                    throw new Exception("User not found for manual processing");
                }

                // For service account processing, we might need to get the user email differently
                // or it might be passed as part of the workflow context
                string userEmail = ActionContext.ContextEmail;

                if (string.IsNullOrEmpty(userEmail))
                {
                    throw new Exception("User email not available for group membership");
                }

                await _googleDirectoryService.AddMemberToGroup(group, userEmail, role, ActionContext.ModuleId, ActionContext.ContextEmail);

                ActionContext.WorkflowItem.Status = (int)eActionStatus.Pass;
            }
            catch (Exception ex)
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

                var user = _userRepository.GetUser(userid);
                string userEmail = user?.Email ?? string.Empty;
                switch (propertyName)
                {
                    case "User Group":
                        // For design-time, always use OAuth (userid > 0 means a user is designing the flow)
                        var groups = await _googleDirectoryService.GetDirectoryGroupsAsync(moduleid, userEmail);
                        if (groups != null)
                        {
                            return new ActionDataResponse
                            {
                                Success = true,
                                Items = groups.Select(g => new ActionDataItem
                                {
                                    Value = g.Email,
                                    Text = g.Name
                                }).ToList()
                            };
                        }
                        break;

                    case "Role":
                        // Return the available group roles as defined in the enum
                        var roles = Enum.GetValues(typeof(Models.eGroupRole))
                            .Cast<Models.eGroupRole>()
                            .Select(r => r.ToString())
                            .ToList();

                        return new ActionDataResponse
                        {
                            Success = true,
                            Items = roles.Select(role => new ActionDataItem
                            {
                                Value = role,
                                Text = role
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
