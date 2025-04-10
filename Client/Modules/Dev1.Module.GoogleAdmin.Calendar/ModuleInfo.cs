using Oqtane.Models;
using Oqtane.Modules;

namespace Dev1.Module.GoogleAdmin.Calendar
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Google Calendar",
            Description = "For Accessing & Working with the Google Calendar, including events",
            Version = "1.0.0",
            ServerManagerType = "Dev1.Module.GoogleAdmin.Manager.GoogleAdminManager, Dev1.Module.GoogleAdmin.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "Dev1.Module.GoogleAdmin.Shared.Oqtane,Dev1.Flow.Core,Radzen.Blazor,NodaTime",
            PackageName = "Dev1.Module.GoogleAdmin",
        };
    }
}
