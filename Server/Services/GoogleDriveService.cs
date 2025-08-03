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
using Oqtane.Enums;
using Oqtane.Models;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class ServerGoogleDriveService : IGoogleDriveService
    {
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Alias _alias;
        private readonly IGoogleCredentials _googleCredentials;
        private readonly IFileRepository _fileRepository;
        private readonly ISettingRepository _settingRepository;

        public ServerGoogleDriveService(
            IUserPermissions userPermissions, 
            ITenantManager tenantManager,
            ISettingRepository settingRepository,
            IGoogleCredentials googleCredentials,
            IFileRepository fileRepository,
            ILogManager logger, 
            IHttpContextAccessor httpContextAccessor)
        {
            _userPermissions = userPermissions;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _alias = tenantManager.GetAlias();
            _settingRepository = settingRepository;
            _googleCredentials = googleCredentials;
            _fileRepository = fileRepository;
        }

        public IList<Drive> GetDriveAsync(int moduleId)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                throw new UnauthorizedAccessException("Unauthorized access to module.");
            }

            try
            {
                var driveService = CreateDriveService();

                var driveRequest = driveService.Drives.List();
                var drives = driveRequest.Execute();

                return drives.Drives;
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error getting drives for module {ModuleId}: {Error}", moduleId, ex.Message);
                throw new Exception($"Google Drive API Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error getting drives for module {ModuleId}: {Error}", moduleId, ex.Message);
                throw;
            }
        }

        public async Task<IList<Google.Apis.Drive.v3.Data.File>> GetFoldersAsync(int moduleId, string parentFolderId = "Website Uploads")
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                throw new UnauthorizedAccessException("Unauthorized access to module.");
            }

            try
            {
                var driveService = CreateDriveService();

                var listRequest = driveService.Files.List();
                listRequest.Q = "mimeType='application/vnd.google-apps.folder'";
                listRequest.Fields = "files(id, name)";
                listRequest.SupportsAllDrives = true;
                listRequest.IncludeItemsFromAllDrives = true;
                listRequest.Spaces = "drive";

                var result = await listRequest.ExecuteAsync();
                return result.Files;
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error getting folders for module {ModuleId}: {Error}", moduleId, ex.Message);
                throw new Exception($"Google Drive API Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error getting folders for module {ModuleId}: {Error}", moduleId, ex.Message);
                throw;
            }
        }

        public async Task<string> UploadFileAsync(int moduleId, string fileName, string contentType, string base64FileData, string folderId = "root")
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                throw new UnauthorizedAccessException("Unauthorized to upload files.");
            }

            if (string.IsNullOrEmpty(base64FileData))
            {
                throw new ArgumentException("No file data was provided");
            }

            try
            {
                var driveService = CreateDriveService();
                byte[] fileBytes = Convert.FromBase64String(base64FileData);
                
                using var fileStream = new MemoryStream(fileBytes);

                var driveFile = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    Parents = new List<string> { folderId }
                };

                var insertRequest = driveService.Files.Create(driveFile, fileStream, contentType);
                insertRequest.SupportsAllDrives = true;
                insertRequest.Fields = "id, webViewLink";

                Google.Apis.Drive.v3.Data.File returnedFile = null;
                insertRequest.ResponseReceived += f => returnedFile = f;

                var result = await insertRequest.UploadAsync();

                if (result.Status != UploadStatus.Completed)
                {
                    throw result.Exception ?? new Exception("Upload failed with unknown error");
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Create, "File {FileName} uploaded to Google Drive", fileName);
                return returnedFile.WebViewLink;
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error uploading file {FileName}: {Error}", fileName, ex.Message);
                throw new Exception($"Google Drive API Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error uploading file {FileName}: {Error}", fileName, ex.Message);
                throw;
            }
        }

        public string DownloadFile(string fileId)
        {
            try
            {
                var driveService = CreateDriveService();
                var request = driveService.Files.Get(fileId);
                var stream = new MemoryStream();

                request.MediaDownloader.ProgressChanged += progress =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            _logger.Log(LogLevel.Debug, this, LogFunction.Other, "Download progress: {Bytes} bytes", progress.BytesDownloaded);
                            break;
                        case DownloadStatus.Completed:
                            _logger.Log(LogLevel.Debug, this, LogFunction.Other, "Download completed for file {FileId}", fileId);
                            break;
                        case DownloadStatus.Failed:
                            _logger.Log(LogLevel.Error, this, LogFunction.Other, "Download failed for file {FileId}", fileId);
                            break;
                    }
                };

                request.Download(stream);
                byte[] fileBytes = stream.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error downloading file {FileId}: {Error}", fileId, ex.Message);
                throw new Exception($"Google Drive API Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error downloading file {FileId}: {Error}", fileId, ex.Message);
                throw;
            }
        }

        private DriveService CreateDriveService()
        {
            var scopes = new[] { 
                DriveService.Scope.Drive, 
                DriveService.Scope.DriveFile, 
                DriveService.Scope.DriveReadonly,
                DriveService.Scope.DriveMetadata,
                DriveService.Scope.DriveAppdata 
            };

            var credential = _googleCredentials.GetServiceAccountCredential(scopes);
            var applicationName = GetApplicationName();

            return new DriveService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });
        }

        private string GetApplicationName()
        {
            var settings = _settingRepository.GetSettings("Site");
            var serviceKey = settings.FirstOrDefault(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey");
            
            if (serviceKey != null && !string.IsNullOrEmpty(serviceKey.SettingValue))
            {
                try
                {
                    var creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);
                    return creds.project_id ?? "Oqtane Google Drive Module";
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Other, "Failed to parse service account for app name: {Error}", ex.Message);
                }
            }
            
            return "Oqtane Google Drive Module";
        }
    }
}
