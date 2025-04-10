using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Dev1.Module.GoogleAdmin.Repository;
using Oqtane.Controllers;
using System.Net;
using Dev1.Module.GoogleAdmin.Services;
using System.Threading.Tasks;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Cors;
using Oqtane.Models;
using System.IO;

namespace Dev1.Module.GoogleAdmin.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class GoogleDriveController : ModuleControllerBase
    {
        private readonly IGoogleDriveService _GoogleDriveService;

        public GoogleDriveController(IGoogleDriveService GoogleDriveService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _GoogleDriveService = GoogleDriveService;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public IList<Drive> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return _GoogleDriveService.GetDriveAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Drive Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }


        // GET: api/GoogleDrive/GetFolders/{moduleid}?parentFolderId={parentFolderId}
        [HttpGet("GetFolders/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IList<Google.Apis.Drive.v3.Data.File>> GetFolders(string moduleid, string parentFolderId = "root")
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _GoogleDriveService.GetFoldersAsync(ModuleId, parentFolderId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Drive Folders Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // ... existing code ...

        //// GET: api/GoogleDrive/UploadFile/{moduleid}/{fileid}?folderId={folderId}
        //[HttpGet("UploadFile/{moduleid}/{fileid}/{folderId}")]
        //[Authorize(Policy = PolicyNames.ViewModule)]
        //public async Task<Google.Apis.Drive.v3.Data.File> UploadFile(string moduleid, string fileid, string folderId = "root")
        //{
        //    int ModuleId, FileId;
        //    if (int.TryParse(moduleid, out ModuleId) &&
        //        int.TryParse(fileid, out FileId) &&
        //        IsAuthorizedEntityId(EntityNames.Module, ModuleId))
        //    {
        //        return await _GoogleDriveService.UploadFileFromOqtaneAsync(ModuleId, FileId, folderId);
        //    }
        //    else
        //    {
        //        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Drive Upload Attempt {ModuleId}", moduleid);
        //        HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        //        return null;
        //    }
        //}

        //// POST: api/GoogleDrive/UploadStream/{moduleid}
        //[HttpPost("UploadStream/{moduleid}/{folderId}")]
        //[Authorize(Policy = PolicyNames.ViewModule)]
        //public async Task<IActionResult> UploadStream(string moduleid, [FromForm] IFormFile file, string folderId = "root")
        //{
        //    int ModuleId;
        //    if (int.TryParse(moduleid, out ModuleId) &&
        //        IsAuthorizedEntityId(EntityNames.Module, ModuleId))
        //    {
        //        using var stream = file.OpenReadStream();
        //        await _GoogleDriveService.UploadFileFromStreamAsync(
        //            ModuleId,
        //            file.FileName,
        //            file.ContentType,
        //            stream,
        //            folderId);


        //    }
        //    else
        //    {
        //        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Drive Upload Attempt {ModuleId}", moduleid);
        //        HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        //    }
        //    return NoContent();
        //}


    }
}
