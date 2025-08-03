using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Dev1.Module.GoogleAdmin.Shared.Models
{
    /// <summary>
    /// Defines the authentication mode for accessing Google Calendars
    /// </summary>
    public enum CalendarAuthMode
    {
        /// <summary>
        /// Use service account to access organization calendars
        /// </summary>
        [Display(Name ="Organisation")]
        OrganizationCalendar,

        /// <summary>
        /// Use OAuth2 user tokens to access user's personal calendars
        /// </summary>
        [Display(Name = "User")]
        UserCalendar
    }
}
