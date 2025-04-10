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

namespace Dev1.Module.GoogleAdmin.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class GoogleAdminController : ModuleControllerBase
    {
        private readonly IGoogleAdminRepository _GoogleAdminRepository;

        public GoogleAdminController(IGoogleAdminRepository GoogleAdminRepository, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _GoogleAdminRepository = GoogleAdminRepository;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public IEnumerable<Shared.Models.GoogleAdmin> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return _GoogleAdminRepository.GetGoogleAdmins(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public Shared.Models.GoogleAdmin Get(int id)
        {
            Shared.Models.GoogleAdmin GoogleAdmin = _GoogleAdminRepository.GetGoogleAdmin(id);
            if (GoogleAdmin != null && IsAuthorizedEntityId(EntityNames.Module, GoogleAdmin.ModuleId))
            {
                return GoogleAdmin;
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Get Attempt {GoogleAdminId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Shared.Models.GoogleAdmin Post([FromBody] Shared.Models.GoogleAdmin GoogleAdmin)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, GoogleAdmin.ModuleId))
            {
                GoogleAdmin = _GoogleAdminRepository.AddGoogleAdmin(GoogleAdmin);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "GoogleAdmin Added {GoogleAdmin}", GoogleAdmin);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Post Attempt {GoogleAdmin}", GoogleAdmin);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                GoogleAdmin = null;
            }
            return GoogleAdmin;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Shared.Models.GoogleAdmin Put(int id, [FromBody] Shared.Models.GoogleAdmin GoogleAdmin)
        {
            if (ModelState.IsValid && GoogleAdmin.GoogleAdminId == id && IsAuthorizedEntityId(EntityNames.Module, GoogleAdmin.ModuleId) && _GoogleAdminRepository.GetGoogleAdmin(GoogleAdmin.GoogleAdminId, false) != null)
            {
                GoogleAdmin = _GoogleAdminRepository.UpdateGoogleAdmin(GoogleAdmin);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "GoogleAdmin Updated {GoogleAdmin}", GoogleAdmin);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Put Attempt {GoogleAdmin}", GoogleAdmin);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                GoogleAdmin = null;
            }
            return GoogleAdmin;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public void Delete(int id)
        {
            Shared.Models.GoogleAdmin GoogleAdmin = _GoogleAdminRepository.GetGoogleAdmin(id);
            if (GoogleAdmin != null && IsAuthorizedEntityId(EntityNames.Module, GoogleAdmin.ModuleId))
            {
                _GoogleAdminRepository.DeleteGoogleAdmin(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "GoogleAdmin Deleted {GoogleAdminId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Delete Attempt {GoogleAdminId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
