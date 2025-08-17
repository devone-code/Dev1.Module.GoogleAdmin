using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Dev1.Module.GoogleAdmin.Shared.Models;
using System;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class GoogleDirectoryService : ServiceBase, IGoogleDirectoryService
    {
        public GoogleDirectoryService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("GoogleDirectory");

        public async Task<CalendarAuthInfo> GetDirectoryAuthInfoAsync(int moduleId, string userEmail)
        {
            var url = $"{ApiUrl}/authinfo?moduleid={moduleId}&useremail={Uri.EscapeDataString(userEmail)}";
            return await GetJsonAsync<CalendarAuthInfo>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<IList<Group>> GetDirectoryGroupsAsync(int moduleId, string userEmail)
        {
            var url = $"{ApiUrl}/groups?moduleid={moduleId}&useremail={Uri.EscapeDataString(userEmail)}";
            return await GetJsonAsync<IList<Group>>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }



        public Task<Member> AddMemberToGroup(string groupName, string memberEmail, string role, int moduleId, string userEmail)
        {
            throw new NotImplementedException();
        }

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
