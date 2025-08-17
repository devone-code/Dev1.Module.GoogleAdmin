using Google.Apis.Drive.v3.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Services
{
    public interface IGoogleDriveService
    {
        IList<Drive> GetDriveAsync(int moduleId, string userEmail);
        Task<IList<Google.Apis.Drive.v3.Data.File>> GetFoldersAsync(int moduleId, string userEmail, string parentFolderId = "root");
        Task<string> UploadFileAsync(int moduleId, string userEmail, string fileName, string contentType, string base64FileData, string folderId = "root");
        string DownloadFile(string fileId, string userEmail);
    }
}