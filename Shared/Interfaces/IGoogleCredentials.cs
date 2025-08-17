using Google.Apis.Auth.OAuth2;
using Dev1.Module.GoogleAdmin.Models;
using Dev1.Module.GoogleAdmin.Shared.Models;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Services
{
    public interface IGoogleCredentials
    {
        /// <summary>
        /// Gets information about available authentication methods
        /// </summary>
        Task<CalendarAuthInfo> GetAuthInfoAsync(string userEmail);
        
        /// <summary>
        /// Gets a service account credential for organization-level access
        /// </summary>
        ServiceAccountCredential GetServiceAccountCredential(string[] scopes);
        
        /// <summary>
        /// Gets a user credential, preferring impersonation over OAuth2
        /// </summary>
        Task<GoogleCredential> GetUserGoogleCredentialAsync(string[] scopes, string userEmail);

        /// <summary>
        /// Gets a service account credential with optional user impersonation
        /// </summary>
        GoogleCredential GetGoogleCredentialFromServiceKey(string[] scopes, string delegatedEmailAddress);
    }
}