using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Dev1.Module.GoogleAdmin.Shared.Models
{
    // Maps to the Oqtane migration table name Dev1GoogleAdmin.CalendarWatch
    [Table("Dev1GoogleAdmin.CalendarWatch")]
    public class CalendarWatch : IAuditable
    {
        [Key]
        public int CalendarWatchId { get; set; }
        public int SiteId { get; set; }

        // Logical identity/scope
        public string CalendarId { get; set; }
        public string UserEmail { get; set; }

        // Google channel details
        public string ChannelId { get; set; }
        public string ResourceId { get; set; }
        public string TokenKey { get; set; }
        public string SyncToken { get; set; }

        // Lifecycle/state
        public DateTime ExpirationUtc { get; set; }
        public int RefCount { get; set; }

        // Webhook endpoint used for Google push notifications
        public string WebhookUrl { get; set; }

        // Auditable
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
