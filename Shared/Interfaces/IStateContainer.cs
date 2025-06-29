using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;

namespace Dev1.Module.GoogleAdmin.Services
{
    public interface IStateContainer
    {
        CalendarList Calendars { get; set; }
        IList<File> DriveFolders { get; set; }
        IList<Group> UserGroups { get; set; }

        event Action OnChange;
    }
}