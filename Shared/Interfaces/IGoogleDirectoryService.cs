using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Services
{
    public interface IGoogleDirectoryService
    {
        IList<Group> GetDirectoryGroupsAsync(int ModuleId);

        Task<Member> AddMemberToGroup(string groupName, string memberEmail, string role,int ModuleId);
    }
}