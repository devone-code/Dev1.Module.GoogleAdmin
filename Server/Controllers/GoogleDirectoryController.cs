//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System.Collections.Generic;
//using Microsoft.AspNetCore.Http;
//using Oqtane.Shared;
//using Oqtane.Enums;
//using Oqtane.Infrastructure;
//using Dev1.Module.GoogleAdmin.Repository;
//using Oqtane.Controllers;
//using System.Net;
//using Dev1.Module.GoogleAdmin.Services;
//using System.Threading.Tasks;
//using Google.Apis.Admin.Directory.directory_v1.Data;

//namespace Dev1.Module.GoogleAdmin.Controllers
//{
//    [Route(ControllerRoutes.ApiRoute)]
//    public class GoogleDirectoryController : ModuleControllerBase
//    {
//        private readonly IGoogleDirectoryService _GoogleDirectoryService;

//        public GoogleDirectoryController(IGoogleDirectoryService GoogleDirectoryService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
//        {
//            _GoogleDirectoryService = GoogleDirectoryService;
//        }

//        // GET: api/<controller>?moduleid=x
//        [HttpGet]
//        [Authorize(Policy = PolicyNames.ViewModule)]
//        public async Task<IList<Group>> Get(string moduleid)
//        {
//            int ModuleId;
//            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
//            {
//                return await _GoogleDirectoryService.GetDirectoryGroupsAsync(ModuleId);
//            }
//            else
//            {
//                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Directory Get Attempt {ModuleId}", moduleid);
//                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
//                return null;
//            }
//        }



//        [HttpPost]
//        [Authorize(Policy = PolicyNames.EditModule)]
//        public async Task<Member> Post(string groupName, string memberEmail, string role, int moduleId, [FromBody] Member member)
//        {
//            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, moduleId))
//            {
//                Member newMember = await _GoogleDirectoryService.AddMemberToGroup(groupName, memberEmail, role,moduleId);
//                _logger.Log(LogLevel.Information, this, LogFunction.Create, $"Added Google Group User {memberEmail} to {groupName}");
//                return newMember;
//            }
//            else
//            {
//                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Directory Add Group Member Attempt");
//                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

//            }
//            return null;
//        }


//    }
//}
