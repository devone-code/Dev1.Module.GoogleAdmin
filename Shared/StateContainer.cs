using System;
using System.Collections.Generic;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Calendar.v3.Data;

namespace Dev1.Module.GoogleAdmin.Shared
{
    public class StateContainer
    {
        private IList<Group> userGroups;

        public IList<Group> UserGroups
        {
            get => userGroups ?? null;
            set
            {
                userGroups = value;
                NotifyStateChanged();
            }
        }


        private IList<File> driveFolders;

        public IList<File> DriveFolders
        {
            get => driveFolders ?? null;
            set
            {
                driveFolders = value;
                NotifyStateChanged();
            }
        }


        private CalendarList calendars;

        public CalendarList Calendars
        {
            get => calendars ?? null;
            set
            {
                calendars = value;
                NotifyStateChanged();
            }
        }


        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
