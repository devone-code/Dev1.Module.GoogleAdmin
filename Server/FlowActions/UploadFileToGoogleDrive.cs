using Dev1.Flow.Core;
using Dev1.Flow.Core.DTOs;
using Dev1.Flow.Core.Helpers;
using Dev1.Flow.Core.Models;
using Dev1.Module.GoogleAdmin.Models;
using Dev1.Module.GoogleAdmin.Services;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.GoogleAction
{
    public class UploadFileToGoogleDrive : IFlowProcessor
    {
        public string ActionName => "Upload file to Google";
        public string ActionDescription => "Upload to a file to the specified Google Drive Folder (requires the user processing this item to be logged in via Google).";

        public List<string> ContextRequirements => new List<string>();

        private readonly IGoogleDriveService _googleDriveService;

        private readonly IUserRepository _userRepository;
        public UploadFileToGoogleDrive(IGoogleDriveService googleDriveService,IUserRepository userRepository)
        {
            _googleDriveService = googleDriveService;
            _userRepository = userRepository;
        }

        public List<ActionPropertyDefinition> PropertyDefinitions => new List<ActionPropertyDefinition>
        {
          //new ActionPropertyDefinition
          //      {
          //          Name = "Uploaded File Link",
          //          InputTypeId = Convert.ToInt16(eInputType.Text),
          //          ForceWorkflow = true,
          //          IsForWorkflow = true,
          //          IsRequired = false
          //      },

            new ActionPropertyDefinition
                {
                    Name = "File Name",
                    InputTypeId = Convert.ToInt16(eInputType.ExternalFile),
                    ForceWorkflow = true,
                    IsForWorkflow = true,
                    IsRequired = false,

                },

            new ActionPropertyDefinition
                {
                    Name = "Folder Id",
                    InputTypeId = Convert.ToInt16(eInputType.List),
                    ForceWorkflow = false,
                    IsForWorkflow = false,
                    IsRequired = true
                },

            //new ActionPropertyDefinition
            //    {
            //        Name = "Folder Name",
            //        InputTypeId = Convert.ToInt16(eDataType.String),
            //        ForceWorkflow = false,
            //        IsForWorkflow = false,
            //        IsRequired = false,

            //    },

            //new ActionPropertyDefinition
            //    {
            //        Name = "File Data",
            //        InputTypeId = Convert.ToInt16(eDataType.String),
            //        ForceWorkflow = true,
            //        IsForWorkflow = true,
            //        IsRequired = true,

            //    },

            new ActionPropertyDefinition
                {
                    Name = "Default File Name",
                    InputTypeId = Convert.ToInt16(eInputType.List),
                    ForceWorkflow = false,
                    IsForWorkflow = false,
                    IsRequired = true,

                }
        };

        public async Task ExecuteActionAsync(WorkflowItemDto WorkflowItem, int SiteId,int moduleId,int loggedInUserId, string ContextName, string ContextEmail)
        {
            try
            {
                // Get required properties
                var fileName = FlowActionHelpers.GetItemPropertyValue(WorkflowItem, "File Name");
                var fileData = FlowActionHelpers.GetItemPropertyAdditionalData(WorkflowItem, "File Name");

                var folderId = FlowActionHelpers.GetItemPropertyValue(WorkflowItem, "Folder Id");
                var defaultFileName = FlowActionHelpers.GetItemPropertyValue(WorkflowItem, "Default File Name");

                eDefaultFileName fileNameType;
                Enum.TryParse(defaultFileName,out fileNameType);

                string fullFileName = null;
                switch (fileNameType)
                {
                    case eDefaultFileName.Original:
                        {
                            fullFileName = fileName;
                            break;
                        }
                    case eDefaultFileName.AppendEmail:
                        {
                            var user = _userRepository.GetUser(loggedInUserId);
                            if(user != null)
                                fullFileName = $"{user.Email}_{fileName}";
                            else
                                fullFileName = $"{fileName}";
                            break;
                        }
                    case eDefaultFileName.AppendUserName:
                        {
                            var user = _userRepository.GetUser(WorkflowItem.ProcessedByUserId);
                            if(!String.IsNullOrEmpty(user.DisplayName))
                                fullFileName = $"{user.DisplayName}_{fileName}";
                            else
                                fullFileName = $"{user.Username}_{fileName}";
                            break;
                        }

                }


                if (string.IsNullOrEmpty(fullFileName) || string.IsNullOrEmpty(fileData) || string.IsNullOrEmpty(folderId))
                {
                    throw new Exception("Required properties are missing");
                }


                // Upload from local disk (base64 data)
                // Note: filename and content type should be stored with the base64 data
                // You might want to store these as separate properties or parse them from the fileData
                //string fileName = "uploaded_file.txt"; // You might want to make this configurable
                string contentType = "application/octet-stream"; // You might want to make this configurable

                string result = await _googleDriveService.UploadFileAsync(moduleId, ContextEmail, fullFileName, contentType, fileData, folderId);

                WorkflowItem.Status = (int)eActionStatus.Pass;
                WorkflowItem.LastResponse = $"The google file link is - {result}";
            }
            catch (Exception ex)
            {
                WorkflowItem.LastResponse = ex.Message;
                WorkflowItem.Status = (int)eActionStatus.Fail;
            }
        }

        public async Task<ActionDataResponse> GetActionDataAsync(string propertyName, int moduleid, int userid, int siteId)
        {
            try
            {
                var user = _userRepository.GetUser(userid);
                string userEmail = user?.Email ?? string.Empty;
                switch (propertyName)
                {
                    case "Folder Id":
                        var folders = await _googleDriveService.GetFoldersAsync(moduleid,userEmail);
                        if (folders != null)
                        {
                            return new ActionDataResponse
                            {
                                Success = true,
                                Items = folders.Select(f => new ActionDataItem 
                                { 
                                    Value = f.Id, 
                                    Text = f.Name 
                                }).ToList()
                            };
                        }
                        break;

                    case "Default File Name":
                        // Return the available default file name options as defined in the enum
                        var fileNameOptions = Enum.GetValues(typeof(eDefaultFileName))
                            .Cast<eDefaultFileName>()
                            .Select(f => f.ToString())
                            .ToList();
                        
                        return new ActionDataResponse
                        {
                            Success = true,
                            Items = fileNameOptions.Select(option => new ActionDataItem 
                            { 
                                Value = option, 
                                Text = option 
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
