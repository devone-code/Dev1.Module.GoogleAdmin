
using Dev1.Module.GoogleAdmin.Services;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Services;
using Radzen;
using System;

namespace Dev1.Module.GoogleAdmin.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IGoogleAdminService, GoogleAdminService>();
            services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
            services.AddRadzenComponents();
            //services.AddScoped<IGoogleDirectoryService, GoogleDirectoryService>();
            //services.AddScoped<IGoogleDriveService, GoogleDriveService>();

            // Debug: Check if types exist
            //var stateContainerType = typeof(IStateContainer);
            //var stateContainerImplType = typeof(StateContainer);
            //Console.WriteLine($"IStateContainer type: {stateContainerType.FullName}");
            //Console.WriteLine($"StateContainer type: {stateContainerImplType.FullName}");
            //Console.WriteLine($"Are they in same assembly: {stateContainerType.Assembly == stateContainerImplType.Assembly}");


            services.AddSingleton<IStateContainer, StateContainer>();
            //services.RegisterFlowServices<ClientStartup>();
        }
    }
}
