using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Controllers;
using System.Net;
using Dev1.Module.GoogleAdmin.Services;
using System.Threading.Tasks;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Dev1.Module.GoogleAdmin.Shared.Models;
using System;

namespace Dev1.Module.GoogleAdmin.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class GoogleDirectoryController : ModuleControllerBase
    {
        private readonly IGoogleDirectoryService _googleDirectoryService;

        public GoogleDirectoryController(IGoogleDirectoryService googleDirectoryService, ILogManager logger, IHttpContextAccessor accessor) 
            : base(logger, accessor)
        {
            _googleDirectoryService = googleDirectoryService;
        }

        // GET: api/GoogleDirectory/authinfo?moduleid=x&useremail=x
        //[HttpGet("authinfo")]
        //[Authorize(Policy = PolicyNames.ViewModule)]
        //public async Task<CalendarAuthInfo> GetAuthInfo(int moduleId, string userEmail)
        //{
        //    if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
        //    {
        //        return await _googleDirectoryService.GetDirectoryAuthInfoAsync(moduleId, userEmail);
        //    }
        //    else
        //    {
        //        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Directory Auth Info Get Attempt {ModuleId}", moduleId);
        //        HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        //        return new CalendarAuthInfo { ErrorMessage = "Unauthorized access" };
        //    }
        //}

        // GET: api/GoogleDirectory/groups?moduleid=x&authmode=x&useremail=x
        [HttpGet("groups")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IList<Group>> GetGroups(int moduleId, string userEmail)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleId))
            {
                try
                {
                    return await _googleDirectoryService.GetDirectoryGroupsAsync(moduleId, userEmail);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "Error getting directory groups for module {ModuleId} with {AuthMode} for user {UserEmail}: {Error}", 
                        moduleId, userEmail, ex.Message);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Directory Get Attempt {ModuleId}", moduleId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        //// POST: api/GoogleDirectory/members
        //[HttpPost("members")]
        //[Authorize(Policy = PolicyNames.EditModule)]
        //public async Task<Member> AddMember([FromBody] AddMemberRequest request)
        //{
        //    if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, request.ModuleId))
        //    {
        //        try
        //        {
        //            var member = await _googleDirectoryService.AddMemberToGroup(
        //                request.GroupName, 
        //                request.MemberEmail, 
        //                request.Role, 
        //                request.ModuleId, 
        //                request.AuthMode, 
        //                request.UserEmail);
                    
        //            _logger.Log(LogLevel.Information, this, LogFunction.Create, "Added member {Email} to group {Group} with {AuthMode} for user {UserEmail}", 
        //                request.MemberEmail, request.GroupName, request.AuthMode, request.UserEmail);
                    
        //            return member;
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Log(LogLevel.Error, this, LogFunction.Create, "Error adding member {Email} to group {Group} for module {ModuleId}: {Error}", 
        //                request.MemberEmail, request.GroupName, request.ModuleId, ex.Message);
        //            HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //            return null;
        //        }
        //    }
        //    else
        //    {
        //        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Directory Add Group Member Attempt {ModuleId}", request?.ModuleId);
        //        HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        //        return null;
        //    }
        //}

        public class AddMemberRequest
        {
            public int ModuleId { get; set; }
            public string GroupName { get; set; }
            public string MemberEmail { get; set; }
            public string Role { get; set; }
            public CalendarAuthMode AuthMode { get; set; }
            public string UserEmail { get; set; }
        }
    }
}
