using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Cors.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using CorsService = OrchardCore.Cors.Services.CorsService;

namespace OrchardCore.Cors
{
    public class Startup : StartupBase
    {
        public override int Order => -1;

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseCors();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddSingleton<CorsService>();

            services.TryAddEnumerable(ServiceDescriptor
                .Transient<IConfigureOptions<CorsOptions>, CorsOptionsConfiguration>());
        }
    }
}
