using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin.Services
{
    public interface IGoogleCredentials
    {
        Task<GoogleCredential> GetGoogleCredentialFromAccessToken(string[] scopes);
        GoogleCredential GetGoogleCredentialFromServiceKey(string[] scopes, string delegatedEmailAddress);
        ServiceAccountCredential GetServiceAccountCredentialFromServiceKey(string[] scopes);
    }
}