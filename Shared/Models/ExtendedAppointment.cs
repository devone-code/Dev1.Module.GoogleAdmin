
using System.Collections.Generic;

namespace Dev1.Module.GoogleAdmin.Shared.Models
{

    public class ExtendedAppointment : Appointment
    {
        public string Description { get; set; } = "";
        public string Location { get; set; } = "";
        public bool IsAllDay { get; set; } = false;
        public string Timezone { get; set; } = "Australia/Sydney";
        public List<string> AttendeeEmails { get; set; } = new();
        public string GoogleEventId { get; set; } = ""; // Store the Google Event ID for updates/deletes
    }

}
