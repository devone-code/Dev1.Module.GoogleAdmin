using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using System.Collections.Generic;

namespace Dev1.Module.GoogleAdmin
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            
            Name = "Google Admin",
            Description = "For Accessing Google APIs",
            Version = "1.0.2",
            ServerManagerType = "Dev1.Module.GoogleAdmin.Manager.GoogleAdminManager, Dev1.Module.GoogleAdmin.Server.Oqtane",
            ReleaseVersions = "1.0.0,1.0.1,1.0.2",
            Dependencies = "Dev1.Module.GoogleAdmin.Shared.Oqtane,Radzen.Blazor,NodaTime,"+
            "Google.Apis.Admin.Directory.directory_v1,"+
            "Google.Apis.Auth," +
            "Google.Apis.Calendar.v3," +
            "Google.Apis.Core," +
            "Google.Apis," +
            "Google.Apis.Drive.v3",
            PackageName = "Dev1.Module.GoogleAdmin",
            Resources = new List<Resource>()
            {
               new Script("_content/Radzen.Blazor/Radzen.Blazor.js"),
               new Resource { ResourceType = ResourceType.Script, Url = "_content/Dev1.Module.GoogleAdmin/Module.js" },
               new Resource { ResourceType = ResourceType.Stylesheet, Url = "_content/Dev1.Module.GoogleAdmin/Module.css" },
            }
        };
    }
}
