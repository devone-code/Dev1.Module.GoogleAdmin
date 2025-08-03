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
        Task<CalendarAuthInfo> GetAuthInfoAsync();
        
        /// <summary>
        /// Gets a service account credential for organization-level access
        /// </summary>
        ServiceAccountCredential GetServiceAccountCredential(string[] scopes);
        
        /// <summary>
        /// Gets a user credential from OAuth2 access token for user-level access
        /// </summary>
        Task<GoogleCredential> GetUserGoogleCredentialAsync(string[] scopes);

        // Method still needed by existing GoogleDriveService and GoogleDirectoryService - remove obsolete attribute
        GoogleCredential GetGoogleCredentialFromServiceKey(string[] scopes, string delegatedEmailAddress);
    }
}