using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;

using Dev1.Module.GoogleAdmin.Repository;
using Oqtane.Repository;


namespace Dev1.Module.GoogleAdmin.Services
{
    public class ServerGoogleAdminService : IGoogleAdminService
    {
        private readonly IGoogleAdminRepository _GoogleAdminRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        private readonly ISettingRepository _settingRepo;

        public ServerGoogleAdminService(IGoogleAdminRepository GoogleAdminRepository, 
            IUserPermissions userPermissions, ITenantManager tenantManager, 
            ISettingRepository settingRepo,
            ILogManager logger, IHttpContextAccessor accessor)
        {
            _GoogleAdminRepository = GoogleAdminRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
            _settingRepo = settingRepo;
        }

        public async Task<List<Shared.Models.GoogleAdmin>> GetGoogleAdminsAsync(int ModuleId)
        {

            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return _GoogleAdminRepository.GetGoogleAdmins(ModuleId).ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Get Attempt {ModuleId}", ModuleId);
                return null;
            }
        }

        public Task<Shared.Models.GoogleAdmin> GetGoogleAdminAsync(int GoogleAdminId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_GoogleAdminRepository.GetGoogleAdmin(GoogleAdminId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Get Attempt {GoogleAdminId} {ModuleId}", GoogleAdminId, ModuleId);
                return null;
            }
        }

        public Task<Shared.Models.GoogleAdmin> AddGoogleAdminAsync(Shared.Models.GoogleAdmin GoogleAdmin)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, GoogleAdmin.ModuleId, PermissionNames.Edit))
            {
                GoogleAdmin = _GoogleAdminRepository.AddGoogleAdmin(GoogleAdmin);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "GoogleAdmin Added {GoogleAdmin}", GoogleAdmin);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Add Attempt {GoogleAdmin}", GoogleAdmin);
                GoogleAdmin = null;
            }
            return Task.FromResult(GoogleAdmin);
        }

        public Task<Shared.Models.GoogleAdmin> UpdateGoogleAdminAsync(Shared.Models.GoogleAdmin GoogleAdmin)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, GoogleAdmin.ModuleId, PermissionNames.Edit))
            {
                GoogleAdmin = _GoogleAdminRepository.UpdateGoogleAdmin(GoogleAdmin);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "GoogleAdmin Updated {GoogleAdmin}", GoogleAdmin);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Update Attempt {GoogleAdmin}", GoogleAdmin);
                GoogleAdmin = null;
            }
            return Task.FromResult(GoogleAdmin);
        }

        public Task DeleteGoogleAdminAsync(int GoogleAdminId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _GoogleAdminRepository.DeleteGoogleAdmin(GoogleAdminId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "GoogleAdmin Deleted {GoogleAdminId}", GoogleAdminId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GoogleAdmin Delete Attempt {GoogleAdminId} {ModuleId}", GoogleAdminId, ModuleId);
            }
            return Task.CompletedTask;
        }
    }
}
