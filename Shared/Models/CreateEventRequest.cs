using Dev1.Module.GoogleAdmin.Models;
using System;

namespace Dev1.Module.GoogleAdmin.Shared.Models
{
    public class CreateEventRequest
    {
        public int ModuleId { get; set; }
        public string CalendarId { get; set; }
        public CalendarAuthMode AuthMode { get; set; }
        public string Timezone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Summary { get; set; }
        public string AttendeeName { get; set; }
        public string AttendeeEmail { get; set; }
    }
}