using Microsoft.Extensions.DependencyInjection;
using Oqtane.Services;
using Dev1.Module.GoogleAdmin.Services;
using Radzen;
using Dev1.Module.GoogleAdmin.Shared;

namespace Dev1.Module.GoogleAdmin.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRadzenComponents();
            services.AddRadzenQueryStringThemeService();

            services.AddScoped<IGoogleAdminService, GoogleAdminService>();
            services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
            services.AddScoped<IGoogleDirectoryService, GoogleDirectoryService>();
            services.AddScoped<IGoogleDriveService, GoogleDriveService>();
            services.AddSingleton<StateContainer>();
        }
    }
}
