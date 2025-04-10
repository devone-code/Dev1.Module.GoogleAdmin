using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Enums;
using Oqtane.Repository;
using Dev1.Module.GoogleAdmin.Repository;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Manager
{
    public class GoogleAdminManager : MigratableModuleBase, IInstallable, IPortable, ISearchable
    {
        private readonly IGoogleAdminRepository _GoogleAdminRepository;
        private readonly IDBContextDependencies _DBContextDependencies;

        public GoogleAdminManager(IGoogleAdminRepository GoogleAdminRepository, IDBContextDependencies DBContextDependencies)
        {
            _GoogleAdminRepository = GoogleAdminRepository;
            _DBContextDependencies = DBContextDependencies;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new GoogleAdminContext(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new GoogleAdminContext(_DBContextDependencies), tenant, MigrationType.Down);
        }

        public string ExportModule(Oqtane.Models.Module module)
        {
            string content = "";
            List<Shared.Models.GoogleAdmin> GoogleAdmins = _GoogleAdminRepository.GetGoogleAdmins(module.ModuleId).ToList();
            if (GoogleAdmins != null)
            {
                content = JsonSerializer.Serialize(GoogleAdmins);
            }
            return content;
        }

        public void ImportModule(Oqtane.Models.Module module, string content, string version)
        {
            List<Shared.Models.GoogleAdmin> GoogleAdmins = null;
            if (!string.IsNullOrEmpty(content))
            {
                GoogleAdmins = JsonSerializer.Deserialize<List<Shared.Models.GoogleAdmin>>(content);
            }
            if (GoogleAdmins != null)
            {
                foreach(var GoogleAdmin in GoogleAdmins)
                {
                    _GoogleAdminRepository.AddGoogleAdmin(new Shared.Models.GoogleAdmin { ModuleId = module.ModuleId, Name = GoogleAdmin.Name });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
           var searchContentList = new List<SearchContent>();

           foreach (var GoogleAdmin in _GoogleAdminRepository.GetGoogleAdmins(pageModule.ModuleId))
           {
               if (GoogleAdmin.ModifiedOn >= lastIndexedOn)
               {
                   searchContentList.Add(new SearchContent
                   {
                       EntityName = "Dev1GoogleAdmin",
                       EntityId = GoogleAdmin.GoogleAdminId.ToString(),
                       Title = GoogleAdmin.Name,
                       Body = GoogleAdmin.Name,
                       ContentModifiedBy = GoogleAdmin.ModifiedBy,
                       ContentModifiedOn = GoogleAdmin.ModifiedOn
                   });
               }
           }

           return Task.FromResult(searchContentList);
        }
    }
}
