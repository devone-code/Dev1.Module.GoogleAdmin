using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Repository
{
    public interface IGoogleAdminRepository
    {
        IEnumerable<Shared.Models.GoogleAdmin> GetGoogleAdmins(int ModuleId);
        Shared.Models.GoogleAdmin GetGoogleAdmin(int GoogleAdminId);
        Shared.Models.GoogleAdmin GetGoogleAdmin(int GoogleAdminId, bool tracking);
        Shared.Models.GoogleAdmin AddGoogleAdmin(Shared.Models.GoogleAdmin GoogleAdmin);
        Shared.Models.GoogleAdmin UpdateGoogleAdmin(Shared.Models.GoogleAdmin GoogleAdmin);
        void DeleteGoogleAdmin(int GoogleAdminId);
    }
}
