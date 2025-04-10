using Oqtane.Models;
using Oqtane.Modules;

namespace Dev1.Module.GoogleAdmin
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Google Admin",
            Description = "For Accessing Google APIs",
            Version = "1.0.0",
            ServerManagerType = "Dev1.Module.GoogleAdmin.Manager.GoogleAdminManager, Dev1.Module.GoogleAdmin.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "Dev1.Module.GoogleAdmin.Shared.Oqtane,Dev1.Flow.Core,Radzen.Blazor,NodaTime",
            PackageName = "Dev1.Module.GoogleAdmin",
        };
    }
}
