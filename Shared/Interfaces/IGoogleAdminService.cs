using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Services
{
    public interface IGoogleAdminService 
    {
        Task<List<Shared.Models.GoogleAdmin>> GetGoogleAdminsAsync(int ModuleId);

        Task<Shared.Models.GoogleAdmin> GetGoogleAdminAsync(int GoogleAdminId, int ModuleId);

        Task<Shared.Models.GoogleAdmin> AddGoogleAdminAsync(Shared.Models.GoogleAdmin GoogleAdmin);

        Task<Shared.Models.GoogleAdmin> UpdateGoogleAdminAsync(Shared.Models.GoogleAdmin GoogleAdmin);

        Task DeleteGoogleAdminAsync(int GoogleAdminId, int ModuleId);
    }
}
