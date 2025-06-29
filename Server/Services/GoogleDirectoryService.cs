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


using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Oqtane.Repository;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;
using Oqtane.Modules;
using System.Net;
using System.IO;

namespace Dev1.Module.GoogleAdmin.Services
{


    public class ServerGoogleDirectoryService : IGoogleDirectoryService
    {

        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Oqtane.Models.Alias _alias;
        private readonly IGoogleCredentials _googleCredentials;

        private readonly ISettingRepository _settingRepo;

        public ServerGoogleDirectoryService(
            IUserPermissions userPermissions, ITenantManager tenantManager,
            ISettingRepository settingRepo,
            IGoogleCredentials googleCredentials,
            ILogManager logger, IHttpContextAccessor accessor)
        {

            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
            _settingRepo = settingRepo;
            _googleCredentials = googleCredentials;
        }

        public async Task<IList<Group>> GetDirectoryGroupsAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                DirectoryService directoryService = null;
                //string calendarName = null;
                var settings = _settingRepo.GetSettings("Site");//,Dev1.GoogleAdmin:ServiceKey", ModuleId, "KEY", "");
                var serviceKey = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey").FirstOrDefault();
                var domain = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_domain").FirstOrDefault();
                var adminEmail = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_adminEmail").FirstOrDefault();

                var Scopes = new[] { DirectoryService.Scope.AdminDirectoryGroup, DirectoryService.Scope.AdminDirectoryGroupMember, DirectoryService.Scope.AdminDirectoryUser };

                var credential = _googleCredentials.GetGoogleCredentialFromServiceKey(Scopes, adminEmail.SettingValue);

                Shared.Models.AccountCredentials creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);

                directoryService = new DirectoryService(new Google.Apis.Services.BaseClientService.Initializer()
                {

                    HttpClientInitializer = credential,
                    ApplicationName = creds.project_id,

                });

                try
                {


                    GroupsResource.ListRequest groupRequest = directoryService.Groups.List();
                    groupRequest.Domain = domain.SettingValue;
                    groupRequest.Credential = credential;

                    Groups groups = await groupRequest.ExecuteAsync();


                    return groups.GroupsValue;
                    //var e = await calendarService.Events.List(calendar.Id).ExecuteAsync();
                }
                catch (Google.GoogleApiException e)
                {
                    throw (new System.Exception(e.Message));
                }

            }
            return null;

        }



        public async Task<Member> AddMemberToGroup(string groupName, string memberEmail, string role,int ModuleId)
        {
            try

            {
                DirectoryService directoryService = null;
                //string calendarName = null;
                var settings = _settingRepo.GetSettings("Site");//,Dev1.GoogleAdmin:ServiceKey", ModuleId, "KEY", "");
                var serviceKey = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey").FirstOrDefault();
                var domain = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_domain").FirstOrDefault();
                var adminEmail = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_adminEmail").FirstOrDefault();

                var Scopes = new[] { DirectoryService.Scope.AdminDirectoryGroup, DirectoryService.Scope.AdminDirectoryGroupMember, DirectoryService.Scope.AdminDirectoryUser };

                var credential = _googleCredentials.GetGoogleCredentialFromServiceKey(Scopes, adminEmail.SettingValue);

                Shared.Models.AccountCredentials creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);

                directoryService = new DirectoryService(new Google.Apis.Services.BaseClientService.Initializer()
                {

                    HttpClientInitializer = credential,
                    ApplicationName = creds.project_id,

                });


                //GroupsResource groupRequest = directoryService.Groups.Get().List();
                //groupRequest.Domain = domain.SettingValue;
                //groupRequest.Credential = credential;

               // Groups groups = groupRequest.Execute();


                Member member = new Member()
                {
                    Status = "ACTIVE",
                    Email = memberEmail,
                    Role = role,
                    //DeliverySettings = "ALL_MAIL"
                };

                return await directoryService.Members.Insert(member, groupName).ExecuteAsync();

            }
            catch(Google.GoogleApiException googleException)
            {
                
                throw new System.Exception(googleException.Error.Message);
            }
            catch(System.Exception e)
            {
                throw e;
            }

            //MembersResource.InsertRequest request = new MembersResource.InsertRequest(directoryService, body, groupdId);
            //try
            //{
            //    Google.Apis.Admin.Directory.directory_v1.Data.Member member = request.Execute();
            //    if (member.Id != null)
            //    {
            //        return true;
            //    }
            //}

        }

    }
}
