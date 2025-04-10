using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Dev1.Flow.Core;
using Dev1.Flow.Core.Models;
using Dev1.Module.GoogleAdmin.Services;
using Dev1.Module.Flow.Helpers;
using System;
using Dev1.Module.GoogleAdmin.Models;
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
    public class UploadFileToGoogleDrive : IFlowProcessor
    {
        private readonly IGoogleDriveService _googleDriveService;

        private readonly IUserRepository _userRepository;
        public UploadFileToGoogleDrive(IGoogleDriveService googleDriveService,IUserRepository userRepository)
        {
            _googleDriveService = googleDriveService;
            _userRepository = userRepository;
        }
        public async Task ExecuteActionAsync(Workflow Workflow, WorkflowItem WorkflowItem, int SiteId)
        {
            try
            {

                // Get required properties
                var fileName = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "File Name");
                var fileData = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "File Data");

                var folderId = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Folder Id");
                var defaultFileName = WorkflowHelpers.GetItemPropertyValue(WorkflowItem, "Default File Name");

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

                            fullFileName = $"{Workflow.CreatedBy}_{fileName}";
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

                string result = await _googleDriveService.UploadFileAsync(
                    Workflow.ModuleId,
                    fullFileName,
                    contentType,
                    fileData,
                    folderId);


                WorkflowItem.Status = (int)eActionStatus.Pass;
                WorkflowItem.LastResponse = $"The google file link is - {result}";
            }
            catch (Exception ex)
            {
                WorkflowItem.LastResponse = ex.Message;
                WorkflowItem.Status = (int)eActionStatus.Fail;
            }
        }
    }
}
