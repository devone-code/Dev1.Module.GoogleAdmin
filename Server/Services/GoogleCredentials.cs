using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Oqtane.Repository;
using Microsoft.AspNetCore.Authentication;
using Dev1.Module.GoogleAdmin.Models;
using Oqtane.Infrastructure;
using Oqtane.Enums;
using Dev1.Module.GoogleAdmin.Shared.Models;

using Google.Apis.Services;
using System.Threading;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class GoogleCredentials : IGoogleCredentials
    {
        private readonly ISettingRepository _settingRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogManager _logger;
        
        public GoogleCredentials(
            ISettingRepository settingRepository, 
            IHttpContextAccessor httpContextAccessor,
            ILogManager logger)
        {
            _settingRepository = settingRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<CalendarAuthInfo> GetAuthInfoAsync(string userEmail)
        {
            var authInfo = new CalendarAuthInfo();
            
            try
            {
                var settings = _settingRepository.GetSettings("Site");
                var serviceKey = settings.FirstOrDefault(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey");
                authInfo.ServiceAccountAvailable = serviceKey != null && !string.IsNullOrEmpty(serviceKey.SettingValue);
                
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    authInfo.UserEmail = userEmail;
                    
                    // Check if we can use impersonation
                    if (authInfo.ServiceAccountAvailable && !string.IsNullOrEmpty(authInfo.UserEmail))
                    {
                        //authInfo.ImpersonationAvailable = await TestImpersonationAsync(authInfo.UserEmail);
                        //if (authInfo.ImpersonationAvailable)
                        //{
                            authInfo.UserGoogleAuthenticated = true;
                            _logger.Log(Oqtane.Shared.LogLevel.Information, this, LogFunction.Other, 
                                "Service account impersonation available for user {Email}", authInfo.UserEmail);
                        //}
                    }
                    
                    // Fallback to OAuth check if impersonation not available
                    if (!authInfo.ImpersonationAvailable)
                    {
                        try
                        {
                            var accessToken = await httpContext.GetTokenAsync("access_token");
                            authInfo.OAuth2Available = !string.IsNullOrEmpty(accessToken);
                            if (authInfo.OAuth2Available)
                            {
                                authInfo.UserGoogleAuthenticated = true;
                                _logger.Log(Oqtane.Shared.LogLevel.Information, this, LogFunction.Other, 
                                    "OAuth2 token available for user {Email}", authInfo.UserEmail ?? "unknown");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(Oqtane.Shared.LogLevel.Debug, this, LogFunction.Other, 
                                "OAuth2 token check failed: {Error}", ex.Message);
                            authInfo.OAuth2Available = false;
                        }
                    }
                }
                
                // Set appropriate error messages with detailed guidance
                if (!authInfo.ServiceAccountAvailable && !authInfo.OAuth2Available)
                {
                    authInfo.ErrorMessage = "No Google authentication configured. Please configure service account credentials in site settings or enable OAuth2 external login.";
                }
                else if (authInfo.ServiceAccountAvailable && string.IsNullOrEmpty(authInfo.UserEmail))
                {
                    authInfo.ErrorMessage = "User email not available for Google impersonation. Please ensure user profile contains email address.";
                }
                else if (authInfo.ServiceAccountAvailable && !authInfo.ImpersonationAvailable && !authInfo.OAuth2Available)
                {
                    authInfo.ErrorMessage = $"Domain-wide delegation not configured for user {authInfo.UserEmail}. Please configure domain-wide delegation in Google Workspace Admin Console or enable OAuth2.";
                }
                else if (!authInfo.UserGoogleAuthenticated)
                {
                    authInfo.ErrorMessage = "User not authenticated with Google. Please check authentication configuration.";
                }
            }
            catch (Exception ex)
            {
                _logger.Log(Oqtane.Shared.LogLevel.Error, this, LogFunction.Other, 
                    "Error checking auth info: {Error}", ex.Message);
                authInfo.ErrorMessage = $"Error checking authentication: {ex.Message}";
            }
            
            return authInfo;
        }

        //private async Task<bool> TestImpersonationAsync(string userEmail)
        //{
        //    try
        //    {
        //        // Test with minimal scope to verify impersonation works
        //        var testScopes = new[] { "https://www.googleapis.com/auth/userinfo.email" };
        //        var credential = GetGoogleCredentialFromServiceKey(testScopes, userEmail);
                
        //        // Make a simple API call to verify the credential works
        //        var oauth2Service = new Oauth2Service(new BaseClientService.Initializer()
        //        {
        //            HttpClientInitializer = credential,
        //            ApplicationName = "Oqtane Google Admin Test"
        //        });

        //        // This will throw if impersonation is not properly configured
        //        var userInfo = await oauth2Service.Userinfo.Get().ExecuteAsync();
                
        //        _logger.Log(Oqtane.Shared.LogLevel.Debug, this, LogFunction.Other, 
        //            "Impersonation test successful for {Email}, got user info for {ActualEmail}", 
        //            userEmail, userInfo.Email);
                
        //        return userInfo.Email?.Equals(userEmail, StringComparison.OrdinalIgnoreCase) == true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Log(Oqtane.Shared.LogLevel.Debug, this, LogFunction.Other, 
        //            "Impersonation test failed for {Email}: {Error}", userEmail, ex.Message);
        //        return false;
        //    }
        //}

        public ServiceAccountCredential GetServiceAccountCredential(string[] scopes)
        {
            var settings = _settingRepository.GetSettings("Site");
            var serviceKey = settings.FirstOrDefault(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey");
            
            if (serviceKey == null || string.IsNullOrEmpty(serviceKey.SettingValue))
            {
                throw new InvalidOperationException("Service account key not configured. Please configure the Google service account key in site settings.");
            }

            try
            {
                // In .NET 9, create GoogleCredential first, apply scopes, then extract ServiceAccountCredential
                var googleCredential = GoogleCredential.FromJson(serviceKey.SettingValue);
                
                // Apply scopes to GoogleCredential
                var scopedCredential = googleCredential.CreateScoped(scopes);
                
                // Extract the underlying ServiceAccountCredential
                if (scopedCredential.UnderlyingCredential is ServiceAccountCredential serviceAccountCredential)
                {
                    return serviceAccountCredential;
                }
                else
                {
                    throw new InvalidOperationException("The provided credential is not a service account credential.");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(Oqtane.Shared.LogLevel.Error, this, LogFunction.Other, "Failed to create service account credential: {Error}", ex.Message);
                throw new InvalidOperationException($"Failed to create service account credential: {ex.Message}", ex);
            }
        }

        public async Task<GoogleCredential> GetUserGoogleCredentialAsync(string[] scopes, string userEmail)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            // Get user's email from Oqtane authentication
            //var userEmail = GetCurrentUserEmail();
            
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.Log(Oqtane.Shared.LogLevel.Warning, this, LogFunction.Other, 
                    "User email not available for impersonation, falling back to OAuth");
                return await GetUserGoogleCredentialViaOAuthAsync(scopes);
            }

            try
            {
                // Try service account impersonation first
                var credential = GetGoogleCredentialFromServiceKey(scopes, userEmail);
                
                _logger.Log(Oqtane.Shared.LogLevel.Debug, this, LogFunction.Other, 
                    "Successfully created impersonation credential for user {Email}", userEmail);
                
                return credential;
            }
            catch (Exception ex)
            {
                _logger.Log(Oqtane.Shared.LogLevel.Warning, this, LogFunction.Other, 
                    "Failed to impersonate user {Email}: {Error}. Falling back to OAuth.", userEmail, ex.Message);
                
                // Fallback to original OAuth approach if impersonation fails
                return await GetUserGoogleCredentialViaOAuthAsync(scopes);
            }
        }

        //private string GetCurrentUserEmail()
        //{
        //    var httpContext = _httpContextAccessor.HttpContext;
            
        //    // Try to get email from various claim types in order of preference
        //    var emailClaim = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email) ??
        //                     httpContext?.User?.FindFirst("email") ??
        //                     httpContext?.User?.FindFirst("preferred_username") ??
        //                     httpContext?.User?.FindFirst("upn") ??
        //                     httpContext?.User?.FindFirst("unique_name");
            
        //    var email = emailClaim?.Value;
            
        //    _logger.Log(Oqtane.Shared.LogLevel.Debug, this, LogFunction.Other, 
        //        "Retrieved user email: {Email} from claim type: {ClaimType}", 
        //        email ?? "null", emailClaim?.Type ?? "none");
            
        //    return email;
        //}

        private async Task<GoogleCredential> GetUserGoogleCredentialViaOAuthAsync(string[] scopes)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            try
            {
                var accessToken = await httpContext.GetTokenAsync("access_token");
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new UnauthorizedAccessException("User not authenticated with Google and impersonation failed. Please log in with Google or configure domain-wide delegation.");
                }

                var credential = GoogleCredential.FromAccessToken(accessToken);
                
                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped(scopes);
                }

                _logger.Log(Oqtane.Shared.LogLevel.Debug, this, LogFunction.Other, 
                    "Successfully created OAuth credential for user");

                return credential;
            }
            catch (Exception ex)
            {
                _logger.Log(Oqtane.Shared.LogLevel.Error, this, LogFunction.Other, 
                    "Failed to get OAuth credential: {Error}", ex.Message);
                throw new UnauthorizedAccessException($"Unable to authenticate user with Google: {ex.Message}", ex);
            }
        }

        public GoogleCredential GetGoogleCredentialFromServiceKey(string[] scopes, string delegatedEmailAddress)
        {
            var settings = _settingRepository.GetSettings("Site");
            var serviceKey = settings.FirstOrDefault(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey");
            
            if (serviceKey == null || string.IsNullOrEmpty(serviceKey.SettingValue))
            {
                throw new InvalidOperationException("Service account key not configured. Please configure the Google service account key in site settings.");
            }

            try
            {
                GoogleCredential credential = GoogleCredential.FromJson(serviceKey.SettingValue);
                
                // Create scoped credential
                credential = credential.CreateScoped(scopes);
                
                // If delegation is needed, create with user
                if (!string.IsNullOrEmpty(delegatedEmailAddress))
                {
                    credential = credential.CreateWithUser(delegatedEmailAddress);
                    
                    _logger.Log(Oqtane.Shared.LogLevel.Debug, this, LogFunction.Other, 
                        "Created service account credential with user delegation for {Email}", delegatedEmailAddress);
                }
                else
                {
                    _logger.Log(Oqtane.Shared.LogLevel.Debug, this, LogFunction.Other, 
                        "Created service account credential without user delegation");
                }

                return credential;
            }
            catch (Exception ex)
            {
                _logger.Log(Oqtane.Shared.LogLevel.Error, this, LogFunction.Other, 
                    "Failed to create Google credential from service key for user {Email}: {Error}", 
                    delegatedEmailAddress ?? "none", ex.Message);
                throw new InvalidOperationException($"Failed to create Google credential from service key: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets setup instructions for domain-wide delegation
        /// </summary>
        public string GetDomainWideDelegationInstructions()
        {
            return @"
To enable service account impersonation (domain-wide delegation):

1. Google Cloud Console:
   - Go to IAM & Admin > Service Accounts
   - Find your service account and click on it
   - Go to 'Details' tab
   - Note the 'Unique ID' (Client ID)

2. Google Workspace Admin Console:
   - Go to Security > API Controls > Domain-wide delegation
   - Click 'Add new'
   - Enter the Client ID from step 1
   - Add these OAuth scopes (comma-separated):
     * https://www.googleapis.com/auth/calendar
     * https://www.googleapis.com/auth/admin.directory.group
     * https://www.googleapis.com/auth/admin.directory.group.member
     * https://www.googleapis.com/auth/admin.directory.user
   - Click 'Authorize'

3. Requirements:
   - Users must have email addresses that match their Google Workspace accounts
   - Service account must be from the same Google Cloud project
   - Only Google Workspace Super Admins can configure domain-wide delegation

Note: This allows the service account to impersonate any user in your domain.
Configure appropriate application-level permissions to control access.";
        }
    }
}
