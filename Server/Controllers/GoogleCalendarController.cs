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

namespace Dev1.Module.GoogleAdmin.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class GoogleCalendarController : ModuleControllerBase
    {
        private readonly IGoogleCalendarService _GoogleCalendarService;

        public GoogleCalendarController(IGoogleCalendarService GoogleCalendarService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _GoogleCalendarService = GoogleCalendarService;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Google.Apis.Calendar.v3.Data.Calendar> Get(string moduleid,string impersonateAccount)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _GoogleCalendarService.GetGoogleCalendarAsync(ModuleId, impersonateAccount);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Calendar Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        [HttpGet("/events")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Google.Apis.Calendar.v3.Data.Calendar> GetEvents(string moduleid, string impersonateAccount)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _GoogleCalendarService.GetGoogleCalendarAsync(ModuleId, impersonateAccount);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Google Calendar Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }
    }
}
