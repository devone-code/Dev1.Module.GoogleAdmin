using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Dev1.Module.GoogleAdmin.Shared.Models
{
    /// <summary>
    /// Contains information about available authentication methods for Google Calendar access
    /// </summary>
    public class CalendarAuthInfo
    {
        /// <summary>
        /// Whether service account authentication is available (for organization calendars)
        /// </summary>
        public bool ServiceAccountAvailable { get; set; }

        /// <summary>
        /// Whether OAuth2 authentication is available (for user calendars)
        /// </summary>
        public bool OAuth2Available { get; set; }

        /// <summary>
        /// Whether the current user is authenticated with Google
        /// </summary>
        public bool UserGoogleAuthenticated { get; set; }

        /// <summary>
        /// Error message if authentication setup is incomplete
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
