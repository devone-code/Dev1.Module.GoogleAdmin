using Google.Apis.Drive.v3.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Services
{
    public interface IGoogleDriveService
    {
        IList<Drive> GetDriveAsync(int ModuleId);

        //Task<Google.Apis.Drive.v3.Data.File> UploadFileFromOqtaneAsync(int ModuleId, int FileId, string folderId = "root");

        //Task<File> UploadFileFromDiskAsync(int ModuleId, int FileId);

        /// <summary>
        /// Gets a list of folders within a specified parent folder
        /// </summary>
        /// <param name="moduleId">The module ID for permission checking</param>
        /// <param name="parentFolderId">The ID of the parent folder. Use "root" for root folder</param>
        /// <returns>List of folder files from Google Drive</returns>
        Task<IList<Google.Apis.Drive.v3.Data.File>> GetFoldersAsync(int moduleId, string parentFolderId = "root");



        /// <summary>
        /// Uploads a file to Google Drive from base64 encoded data
        /// </summary>
        Task<string> UploadFileAsync(
            int moduleId,
            string fileName,
            string contentType,
            string base64FileData,  // Changed from Stream
            string folderId = "root");


        string DownloadFile(string fileId);
    }





}