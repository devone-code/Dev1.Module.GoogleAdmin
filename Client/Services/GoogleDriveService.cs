using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Drive.v3.Data;
using Oqtane.Models;
using System.Net;
using System.Xml.Linq;
using System;
using System.IO;
using static System.Net.WebRequestMethods;
using Dev1.Flow.Core.Models;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class GoogleDriveService : ServiceBase, IGoogleDriveService
    {
        public GoogleDriveService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("GoogleDrive");

        public string DownloadFile(string fileId)
        {
            throw new NotImplementedException();
        }

        public IList<Drive> GetDriveAsync(int ModuleId)
        {
            var drives = GetJsonAsync<IList<Drive>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), null).Result;
            return drives;
        }

        public async Task<IList<Google.Apis.Drive.v3.Data.File>> GetFoldersAsync(int moduleId, string parentFolderId = "root")
        {
            try
            {
                return await GetJsonAsync<IList<Google.Apis.Drive.v3.Data.File>>($"{Apiurl}/GetFolders/{moduleId}?parentFolderId={parentFolderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting folders: {ex.Message}");
                return null;
            }
        }



        public async Task<string> UploadFileAsync(
            int moduleId,
            string fileName,
            string contentType,
            string base64FileData,  // Changed from Stream
            string folderId = "root")
        {
            throw new NotImplementedException();
        }

        //public async Task<Google.Apis.Drive.v3.Data.File> UploadFileFromOqtaneAsync(int ModuleId, IFormFile FileId)
        //{
        //    return await GetJsonAsync<Google.Apis.Drive.v3.Data.File>($"{Apiurl}/upload/local?moduleid={ModuleId}&fileid={FileId}");
        //    //return  await PostJsonAsync<Member>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, ModuleId), null);
        //    //await GetJsonAsync<Member>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), null);

        //}

    }
}
