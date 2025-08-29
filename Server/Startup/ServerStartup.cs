using Dev1.Flow.Core;
using Dev1.Module.GoogleAdmin.Repository;
using Dev1.Module.GoogleAdmin.Services;
using Dev1.Module.GoogleAdmin.Shared;
using GoogleApi.Extensions;
using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Radzen;

namespace Dev1.Module.GoogleAdmin.Startup
{
    public class ServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented

        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //For Flow. Flow requires that you register  a keyed service
            //for your Flow definition. Make sure you give it a unique name
            //otherwise it may clash with other modules that implement Flow.
            //When resolving in your services, you need to do so as:
            //[FromKeyedServices("Dev1.GoogleAdmin_FlowInfo")]IFlowInfo flowInfo
            services.AddKeyedScoped<IFlowInfo, FlowInfo>("Dev1.GoogleAdmin_FlowInfo");


            services.AddScoped<IGoogleAdminService, ServerGoogleAdminService>();
            services.AddScoped<IGoogleCalendarService, ServerGoogleCalendarService>();
            services.AddScoped<IGoogleDirectoryService, ServerGoogleDirectoryService>();
            services.AddScoped<IGoogleDriveService, ServerGoogleDriveService>();
            services.AddScoped<IGoogleCredentials, GoogleCredentials>();

            services.AddRadzenComponents();
            services.AddRadzenQueryStringThemeService();


            services.AddDbContextFactory<GoogleAdminContext>(opt => { }, ServiceLifetime.Transient);

            services.AddScoped<IStateContainer,StateContainer>();

            services
                .AddGoogleApiClients();
        }
    }
}
