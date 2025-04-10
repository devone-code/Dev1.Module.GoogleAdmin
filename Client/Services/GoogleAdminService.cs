using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class GoogleAdminService : ServiceBase, IGoogleAdminService
    {
        public GoogleAdminService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("GoogleAdmin");

        public async Task<List<Shared.Models.GoogleAdmin>> GetGoogleAdminsAsync(int ModuleId)
        {
            List<Shared.Models.GoogleAdmin> GoogleAdmins = await GetJsonAsync<List<Shared.Models.GoogleAdmin>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Shared.Models.GoogleAdmin>().ToList());
            return GoogleAdmins.OrderBy(item => item.Name).ToList();
        }

        public async Task<Shared.Models.GoogleAdmin> GetGoogleAdminAsync(int GoogleAdminId, int ModuleId)
        {
            return await GetJsonAsync<Shared.Models.GoogleAdmin>(CreateAuthorizationPolicyUrl($"{Apiurl}/{GoogleAdminId}", EntityNames.Module, ModuleId));
        }

        public async Task<Shared.Models.GoogleAdmin> AddGoogleAdminAsync(Shared.Models.GoogleAdmin GoogleAdmin)
        {
            return await PostJsonAsync<Shared.Models.GoogleAdmin>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, GoogleAdmin.ModuleId), GoogleAdmin);
        }

        public async Task<Shared.Models.GoogleAdmin> UpdateGoogleAdminAsync(Shared.Models.GoogleAdmin GoogleAdmin)
        {
            return await PutJsonAsync<Shared.Models.GoogleAdmin>(CreateAuthorizationPolicyUrl($"{Apiurl}/{GoogleAdmin.GoogleAdminId}", EntityNames.Module, GoogleAdmin.ModuleId), GoogleAdmin);
        }

        public async Task DeleteGoogleAdminAsync(int GoogleAdminId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{GoogleAdminId}", EntityNames.Module, ModuleId));
        }
    }
}
