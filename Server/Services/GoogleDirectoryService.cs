using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using Oqtane.Repository;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;
using System;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class ServerGoogleDirectoryService : IGoogleDirectoryService
    {
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Oqtane.Models.Alias _alias;
        private readonly IGoogleCredentials _googleCredentials;
        private readonly ISettingRepository _settingRepository;

        public ServerGoogleDirectoryService(
            IUserPermissions userPermissions, 
            ITenantManager tenantManager,
            ISettingRepository settingRepository,
            IGoogleCredentials googleCredentials,
            ILogManager logger, 
            IHttpContextAccessor httpContextAccessor)
        {
            _userPermissions = userPermissions;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _alias = tenantManager.GetAlias();
            _settingRepository = settingRepository;
            _googleCredentials = googleCredentials;
        }

        public async Task<IList<Group>> GetDirectoryGroupsAsync(int moduleId, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                throw new UnauthorizedAccessException("Unauthorized access to module.");
            }

            try
            {
                var directoryService = await CreateDirectoryServiceAsync(userEmail);
                var domain = GetDomain();

                var groupRequest = directoryService.Groups.List();
                groupRequest.Domain = domain;

                var groups = await groupRequest.ExecuteAsync();
                return groups.GroupsValue;
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Google API error getting directory groups for module {ModuleId} for user {UserEmail}: {Error}", 
                    moduleId, userEmail, ex.Message);
                throw new Exception($"Google Directory API Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error getting directory groups for module {ModuleId} for user {UserEmail}: {Error}", 
                    moduleId, userEmail, ex.Message);
                throw;
            }
        }

        public async Task<Member> AddMemberToGroup(string groupName, string memberEmail, string role, int moduleId, string userEmail)
        {
            if (!_userPermissions.IsAuthorized(_httpContextAccessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                throw new UnauthorizedAccessException("Unauthorized to edit directory groups.");
            }

            try
            {
                var directoryService = await CreateDirectoryServiceAsync(userEmail);

                var member = new Member
                {
                    Status = "ACTIVE",
                    Email = memberEmail,
                    Role = role
                };

                var result = await directoryService.Members.Insert(member, groupName).ExecuteAsync();
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Member {Email} added to group {Group} by user {UserEmail}", 
                    memberEmail, groupName, userEmail);
                
                return result;
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Google API error adding member {Email} to group {Group} for user {UserEmail}: {Error}", 
                    memberEmail, groupName, userEmail, ex.Message);
                throw new Exception($"Google Directory API Error: {ex.Error?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error adding member {Email} to group {Group} for user {UserEmail}: {Error}", 
                    memberEmail, groupName, userEmail, ex.Message);
                throw;
            }
        }

        private async Task<DirectoryService> CreateDirectoryServiceAsync(string userEmail)
        {
            var scopes = new[] { 
                DirectoryService.Scope.AdminDirectoryGroup, 
                DirectoryService.Scope.AdminDirectoryGroupMember, 
                DirectoryService.Scope.AdminDirectoryUser 
            };

            var credential = await _googleCredentials.GetUserGoogleCredentialAsync(scopes, userEmail);
            var applicationName = GetApplicationName();

            return new DirectoryService(new Google.Apis.Services.BaseClientService.Initializer
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
                    return creds.project_id ?? "Oqtane Google Directory Module";
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Other, "Failed to parse service account for app name: {Error}", ex.Message);
                }
            }
            
            return "Oqtane Google Directory Module";
        }

        private string GetDomain()
        {
            var settings = _settingRepository.GetSettings("Site");
            var domain = settings.FirstOrDefault(x => x.SettingName == "Dev1.GoogleAdmin:_domain");
            return domain?.SettingValue ?? "";
        }
    }
}
