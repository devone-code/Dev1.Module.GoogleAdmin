using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Dev1.Module.GoogleAdmin.Repository;
using Dev1.Module.GoogleAdmin.Services;
using Dev1.Flow.Core;
using Radzen;
using Dev1.Module.GoogleAdmin.Shared;

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

            services.AddScoped<IGoogleAdminService, ServerGoogleAdminService>();
            services.AddScoped<IGoogleCalendarService, ServerGoogleCalendarService>();
            services.AddScoped<IGoogleDirectoryService, ServerGoogleDirectoryService>();
            services.AddScoped<IGoogleDriveService, ServerGoogleDriveService>();
            services.AddScoped<IGoogleCredentials, GoogleCredentials>();
            


            services.AddRadzenComponents();
            services.AddRadzenQueryStringThemeService();


            services.AddDbContextFactory<GoogleAdminContext>(opt => { }, ServiceLifetime.Transient);

            services.AddScoped<IStateContainer,StateContainer>();


            services.RegisterFlowServices<ServerStartup>();
        }
    }
}
