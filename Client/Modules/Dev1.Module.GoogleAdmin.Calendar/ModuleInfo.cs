using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using System.Collections.Generic;

namespace Dev1.Module.GoogleAdmin.Calendar
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Google Calendar",
            Description = "For Accessing & Working with the Google Calendar, including events",
            Version = "1.0.2",
            ServerManagerType = "Dev1.Module.GoogleAdmin.Manager.GoogleAdminManager, Dev1.Module.GoogleAdmin.Server.Oqtane",
            ReleaseVersions = "1.0.0,1.0.1,1.0.2",
            Dependencies = "Dev1.Module.GoogleAdmin.Shared.Oqtane,Radzen.Blazor,NodaTime,"+
            "Google.Apis.Admin.Directory.directory_v1," +
            "Google.Apis.Auth," +
            "Google.Apis.Calendar.v3," +
            "Google.Apis.Core," +
            "Google.Apis," +
            "Google.Apis.Drive.v3",
            PackageName = "Dev1.Module.GoogleAdmin",
            Resources = new List<Resource>()
            {
               new Script("_content/Radzen.Blazor/Radzen.Blazor.js"),
               new Resource { ResourceType = ResourceType.Script, Url = "_content/Dev1.Module.GoogleAdmin/Calendar.js" },
               new Resource { ResourceType = ResourceType.Stylesheet, Url = "_content/Dev1.Module.GoogleAdmin/Calendar.css" },
            }
        };
    }
}
