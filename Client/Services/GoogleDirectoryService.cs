using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using Google.Apis.Admin.Directory.directory_v1.Data;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class GoogleDirectoryService : ServiceBase, IGoogleDirectoryService
    {
        public GoogleDirectoryService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("GoogleDirectory");

        public IList<Group> GetDirectoryGroupsAsync(int ModuleId)
        {
            var groups = GetJsonAsync<IList<Group>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId),null).Result;
            return groups;
        }

        public async Task<Member> AddMemberToGroup(string groupName, string memberEmail, string role,int ModuleId)
        {
            throw new System.NotImplementedException();
            //return  await PostJsonAsync<Member>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, ModuleId), null);
            //await GetJsonAsync<Member>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), null);

        }



    }
}
