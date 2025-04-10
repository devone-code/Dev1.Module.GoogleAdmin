using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Infrastructure;
using Oqtane.Security;
using Oqtane.Shared;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Oqtane.Repository;
using System.Text.Json;
using System.IO;
using Google.Apis.Upload;
using System;
using Google.Apis.Download;


namespace Dev1.Module.GoogleAdmin.Services
{


    public class ServerGoogleDriveService : IGoogleDriveService
    {

        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Oqtane.Models.Alias _alias;
        private readonly IGoogleCredentials _googleCredentials;
        private readonly IFileRepository _files;

        private readonly ISettingRepository _settingRepo;

        public ServerGoogleDriveService(
            IUserPermissions userPermissions, ITenantManager tenantManager,
            ISettingRepository settingRepo,
            IGoogleCredentials googleCredentials,
            IFileRepository files,
            ILogManager logger, IHttpContextAccessor accessor)
        {

            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
            _settingRepo = settingRepo;
            _googleCredentials = googleCredentials;
            _files = files;
        }

        public IList<Drive> GetDriveAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                DriveService driveService = null;
                //string calendarName = null;
                var settings = _settingRepo.GetSettings("Site");//,Dev1.GoogleAdmin:ServiceKey", ModuleId, "KEY", "");
                var serviceKey = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey").FirstOrDefault();
                //var domain = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_domain").FirstOrDefault();
                var adminEmail = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_adminEmail").FirstOrDefault();

                var Scopes = new[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile };

                var credential = _googleCredentials.GetGoogleCredentialFromServiceKey(Scopes, adminEmail.SettingValue);

                Shared.Models.AccountCredentials creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);

                driveService = new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
                {

                    HttpClientInitializer = credential,
                    ApplicationName = creds.project_id,

                });

                try
                {


                    DrivesResource.ListRequest driveRequest = driveService.Drives.List();
                    //driveRequest.Domain = domain.SettingValue;
                    driveRequest.Credential = credential;

                    DriveList drives = driveRequest.Execute();


                    return drives.Drives;
                    //var e = await calendarService.Events.List(calendar.Id).ExecuteAsync();
                }
                catch (Google.GoogleApiException e)
                {
                    throw (new System.Exception(e.Message));
                }

            }
            return null;

        }

        private DriveService InitializeDriveService()
        {
            var settings = _settingRepo.GetSettings("Site");
            var serviceKey = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey").FirstOrDefault();
            //var adminEmail = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_adminEmail").FirstOrDefault();

            var Scopes = new[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile, DriveService.Scope.DriveReadonly,
                DriveService.Scope.DriveMetadata,
                DriveService.Scope.DriveAppdata };
            Shared.Models.AccountCredentials creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);


            var credential = _googleCredentials.GetServiceAccountCredentialFromServiceKey(Scopes);
            //var credential = _googleCredentials.GetGoogleCredentialFromAccessToken(Scopes).GetAwaiter().GetResult();
            //var credential = _googleCredentials.GetGoogleCredentialFromServiceKey(Scopes);
            //Shared.Models.AccountCredentials creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);


            return new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = creds.project_id,
            });
        }

        public async Task<IList<Google.Apis.Drive.v3.Data.File>> GetFoldersAsync(int moduleId, string parentFolderId = "Website Uploads")
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                try
                {
                    var driveService = InitializeDriveService();

                    var listRequest = driveService.Files.List();
                    listRequest.Q = "mimeType='application/vnd.google-apps.folder'";
                    listRequest.Fields = "files(id, name)";

                    // Add these parameters
                    listRequest.SupportsAllDrives = true;
                    listRequest.IncludeItemsFromAllDrives = true;

                    listRequest.Spaces = "drive";

                    var result = await listRequest.ExecuteAsync();
                    return result.Files;
                }
                catch (Google.GoogleApiException e)
                {
                    throw new Exception(e.Message);
                }
            }
            return null;
        }

        //public async Task<Google.Apis.Drive.v3.Data.File> UploadFileFromOqtaneAsync(int ModuleId, int FileId, string folderId = "root")
        //{
        //    if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
        //    {
        //        try
        //        {
        //            var driveService = InitializeDriveService();
        //            Oqtane.Models.File file = _files.GetFile(FileId);

        //            if (file != null)
        //            {
        //                using var uploadStream = System.IO.File.OpenRead(Path.Combine(file.Folder.Path, file.Name));

        //                Google.Apis.Drive.v3.Data.File driveFile = new Google.Apis.Drive.v3.Data.File
        //                {
        //                    Name = file.Name,
        //                    Parents = new List<string> { folderId }
        //                };

        //                FilesResource.CreateMediaUpload insertRequest = driveService.Files.Create(
        //                    driveFile, uploadStream, file.GetMimeType());

        //                insertRequest.ProgressChanged += Upload_ProgressChanged;
        //                insertRequest.ResponseReceived += Upload_ResponseReceived;

        //                await insertRequest.UploadAsync();

        //                static void Upload_ProgressChanged(IUploadProgress progress) =>
        //                    Console.WriteLine(progress.Status + " " + progress.BytesSent);

        //                Google.Apis.Drive.v3.Data.File returnedFile = null;
        //                void Upload_ResponseReceived(Google.Apis.Drive.v3.Data.File file)
        //                { returnedFile = file; }

        //                return returnedFile;
        //            }
        //            else
        //            {
        //                throw new Exception("File not found in Website");
        //            }
        //        }
        //        catch (Google.GoogleApiException e)
        //        {
        //            throw new Exception(e.Message);
        //        }
        //    }
        //    return null;
        //}

        public async Task<string> UploadFileAsync(
            int moduleId,
            string fileName,
            string contentType,
            string base64FileData,  // Changed from Stream
            string folderId = "root")
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                try
                {
                    var driveService = InitializeDriveService();

                    if (!string.IsNullOrEmpty(base64FileData))
                    {
                        byte[] fileBytes = Convert.FromBase64String(base64FileData);
                        using var fileStream = new MemoryStream(fileBytes);

                        var driveFile = new Google.Apis.Drive.v3.Data.File
                        {
                            Name = fileName,
                            Parents = new List<string> { folderId }
                        };

                        var insertRequest = driveService.Files.Create(
                            driveFile,

                            fileStream,
                            contentType);

                        insertRequest.SupportsAllDrives = true;

                        // Set fields to return file ID and webViewLink
                        insertRequest.Fields = "id, webViewLink";

                        Google.Apis.Drive.v3.Data.File returnedFile = null;
                        insertRequest.ResponseReceived += f => returnedFile = f;

                        var result = await insertRequest.UploadAsync();

                        if (result.Status != UploadStatus.Completed) //check status, now it's throwing here sometimes
                            throw result.Exception;




                        //var uploadedFileLink = workflowItem.WorkflowItemProperties.Where(x => x.Name == "Uploaded File Link").FirstOrDefault();
                        //if (uploadedFileLink != null)
                        return returnedFile.WebViewLink;

                    }
                    else
                    {
                        throw new Exception("No file data was provided");
                    }
                }
                catch (Google.GoogleApiException e)
                {
                    throw new Exception(e.Message);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            else
                throw new Exception("You do not have permission to process this item.");


        }


        //public MemoryStream DownloadFile
        //{
        /// <summary>
        /// Download a Document file in PDF format.
        /// </summary>
        /// <param name="fileId">file ID of any workspace document format file.</param>
        /// <returns>returns base64 Encoded string.</returns>
        public string DownloadFile(string fileId)
        {
            try
            {
                var driveService = InitializeDriveService();

                var request = driveService.Files.Get(fileId);
                var stream = new MemoryStream();

                // Add a handler which will be notified on progress changes.
                // It will notify on each chunk download and when the
                // download is completed or failed.
                request.MediaDownloader.ProgressChanged +=
                    progress =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Downloading:
                                {
                                    Console.WriteLine(progress.BytesDownloaded);
                                    break;
                                }
                            case DownloadStatus.Completed:
                                {
                                    Console.WriteLine("Download complete.");
                                    break;
                                }
                            case DownloadStatus.Failed:
                                {
                                    Console.WriteLine("Download failed.");
                                    break;
                                }
                        }
                    };
                request.Download(stream);


                byte[] fileBytes = stream.ToArray();

                // Convert to Base64 and store in FileDataProperty
                return Convert.ToBase64String(fileBytes);

            }
            catch (Exception e)
            {
                // TODO(developer) - handle error appropriately
                if (e is AggregateException)
                {
                    Console.WriteLine("Credential Not found");
                }
                else
                {
                    throw;
                }
            }
            return null;
        }
        //}


    }
}
