using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Http;
using Oqtane.Repository;
using Oqtane.Security;
using Microsoft.AspNetCore.Authentication;
using System.Threading;
using Google.Apis.Auth.OAuth2.Responses;

namespace Dev1.Module.GoogleAdmin.Services
{
    public class GoogleCredentials : IGoogleCredentials
    {
        private readonly ISettingRepository _settingRepo;
        private readonly IHttpContextAccessor _accessor;
        public GoogleCredentials(IHttpContextAccessor accessor,
            ISettingRepository settingRepo
            )
        {

            _accessor = accessor;
            _settingRepo = settingRepo;
        }

        public GoogleCredential GetGoogleCredentialFromServiceKey(string[] scopes, string delegatedEmailAddress)
        {
            var settings = _settingRepo.GetSettings("Site");//,Dev1.GoogleAdmin:ServiceKey", ModuleId, "KEY", "");
            var serviceKey = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey").FirstOrDefault();
            //var domain = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:_domain").FirstOrDefault();
         
            if (serviceKey != null)
            {

                Shared.Models.AccountCredentials creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);

                //ServiceAccountCredential account = ServiceAccountCredential.(FromJson(serviceKey.SettingValue);//

                // Create an explicit ServiceAccountCredential credential
                //var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(creds.client_email)
                //{
                //var Scopes = new[] { DirectoryService.Scope.AdminDirectoryGroup, DirectoryService.Scope.AdminDirectoryGroupMember, DirectoryService.Scope.AdminDirectoryUser };
                //    //User = "it@ssdp.org.au",
                //    //UniverseDomain = "ssdp.org.au"
                //}.FromPrivateKey(creds.private_key));
                GoogleCredential credential;
                if (delegatedEmailAddress != null)
                {
                    credential = GoogleCredential.FromJson(serviceKey.SettingValue)
                       .CreateScoped(scopes).CreateWithUser(delegatedEmailAddress);
                    //return credential;
                }
                else
                {
                    credential = GoogleCredential.FromJson(serviceKey.SettingValue)
                       .CreateScoped(scopes);
                    //return credential;
                }

                return credential;

            }
            else
                return null;
        }


        public async Task<GoogleCredential> GetGoogleCredentialFromAccessToken(string[] scopes)
        {
            var settings = _settingRepo.GetSettings("Site");//,Dev1.GoogleAdmin:ServiceKey", ModuleId, "KEY", "");
            var serviceKey = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey").FirstOrDefault();


            if (serviceKey != null)
            {
                if (_accessor.HttpContext.User.Identity.IsAuthenticated)
                {

                    var t = await _accessor.HttpContext.GetTokenAsync("access_token");
                    //var rt = await _accessor.HttpContext.GetTokenAsync("refresh_token");

                    GoogleCredential credential = GoogleCredential.FromAccessToken(t).CreateScoped(scopes);
                    return credential;
                }
                else
                    return null;
            }
            else
                return null;
        }


        public ServiceAccountCredential GetServiceAccountCredentialFromServiceKey(string[] scopes)
        {
            var settings = _settingRepo.GetSettings("Site");//,Dev1.GoogleAdmin:ServiceKey", ModuleId, "KEY", "");
            var serviceKey = settings.Where(x => x.SettingName == "Dev1.GoogleAdmin:ServiceKey").FirstOrDefault();


            if (serviceKey != null)
            {

                Shared.Models.AccountCredentials creds = JsonSerializer.Deserialize<Shared.Models.AccountCredentials>(serviceKey.SettingValue);

                //ServiceAccountCredential account = ServiceAccountCredential.(FromJson(serviceKey.SettingValue);//

                // Create an explicit ServiceAccountCredential credential
                ServiceAccountCredential credential = null;

                    credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(creds.client_email)
                    {
                        Scopes = scopes
                    }.FromPrivateKey(creds.private_key));
                

                return credential;
            }
            else
                return null;



        }

    }
}
