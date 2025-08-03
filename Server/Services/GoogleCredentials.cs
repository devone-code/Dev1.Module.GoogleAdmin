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

        public async Task<CalendarAuthInfo> GetAuthInfoAsync()
        {
            var authInfo = new CalendarAuthInfo();
            
            try
            {
                // Check service account availability
                var settings = _settingRepository.GetSettings("Site");
                var serviceKey = settings.FirstOrDefault(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey");
                authInfo.ServiceAccountAvailable = serviceKey != null && !string.IsNullOrEmpty(serviceKey.SettingValue);
                
                // Check OAuth2 availability
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    try
                    {
                        var accessToken = await httpContext.GetTokenAsync("access_token");
                        authInfo.OAuth2Available = !string.IsNullOrEmpty(accessToken);
                        authInfo.UserGoogleAuthenticated = authInfo.OAuth2Available;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(Oqtane.Shared.LogLevel.Debug, this, LogFunction.Other, "OAuth2 token check failed: {Error}", ex.Message);
                        authInfo.OAuth2Available = false;
                        authInfo.UserGoogleAuthenticated = false;
                    }
                }
                else
                {
                    authInfo.OAuth2Available = false;
                    authInfo.UserGoogleAuthenticated = false;
                }
                
                // Set error message if no auth methods available
                if (!authInfo.ServiceAccountAvailable && !authInfo.OAuth2Available)
                {
                    authInfo.ErrorMessage = "No Google authentication configured. Please configure either service account or OAuth2 authentication.";
                }
                else if (!authInfo.ServiceAccountAvailable)
                {
                    authInfo.ErrorMessage = "Service account not configured. Organization calendars will not be available.";
                }
                else if (!authInfo.OAuth2Available)
                {
                    authInfo.ErrorMessage = "User not authenticated with Google. Personal calendars will not be available.";
                }
            }
            catch (Exception ex)
            {
                _logger.Log(Oqtane.Shared.LogLevel.Error, this, LogFunction.Other, "Error checking auth info: {Error}", ex.Message);
                authInfo.ErrorMessage = $"Error checking authentication: {ex.Message}";
            }
            
            return authInfo;
        }

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

        public async Task<GoogleCredential> GetUserGoogleCredentialAsync(string[] scopes)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            try
            {
                // Get the access token from Oqtane's OAuth2 authentication
                var accessToken = await httpContext.GetTokenAsync("access_token");
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new UnauthorizedAccessException("User not authenticated with Google. Please log in with Google.");
                }

                // Create credential from access token
                var credential = GoogleCredential.FromAccessToken(accessToken);
                
                // Ensure it has the required scopes
                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped(scopes);
                }

                return credential;
            }
            catch (Exception ex)
            {
                _logger.Log(Oqtane.Shared.LogLevel.Error, this, LogFunction.Other, "Failed to get user access token: {Error}", ex.Message);
                throw new InvalidOperationException($"Failed to get user access token: {ex.Message}", ex);
            }
        }

        // Method still needed by existing GoogleDriveService and GoogleDirectoryService - remove obsolete attribute
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
                }

                return credential;
            }
            catch (Exception ex)
            {
                _logger.Log(Oqtane.Shared.LogLevel.Error, this, LogFunction.Other, "Failed to create Google credential from service key: {Error}", ex.Message);
                throw new InvalidOperationException($"Failed to create Google credential from service key: {ex.Message}", ex);
            }
        }
    }
}
