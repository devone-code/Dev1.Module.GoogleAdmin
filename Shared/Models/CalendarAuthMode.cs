using System;
using System.ComponentModel.DataAnnotations;

namespace Dev1.Module.GoogleAdmin.Shared.Models
{
    /// <summary>
    /// Defines the authentication mode for accessing Google APIs
    /// </summary>
    public enum CalendarAuthMode
    {
        /// <summary>
        /// Use service account to access organization resources
        /// </summary>
        [Display(Name = "Organization")]
        OrganizationCalendar,

        /// <summary>
        /// Use service account impersonation or OAuth2 for user resources
        /// </summary>
        [Display(Name = "User")]
        UserCalendar,

        /// <summary>
        /// Force OAuth2 authentication (bypass impersonation)
        /// </summary>
        [Display(Name = "OAuth2 Only")]
        OAuth2Only
    }
}
