using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Search.Configuration;
using OrchardCore.Search.Deployment;
using OrchardCore.Search.Drivers;
using OrchardCore.Search.Migrations;
using OrchardCore.Search.Models;
using OrchardCore.Search.ViewModels;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search
{
    /// <summary>
    /// These services are registered on the tenant service collection.
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<SearchSettings>, SearchSettingsConfiguration>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<ISite>, SearchSettingsDisplayDriver>();

            services.AddContentPart<SearchFormPart>()
                    .UseDisplayDriver<SearchFormPartDisplayDriver>();

            services.AddDataMigration<SearchMigrations>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Search",
                areaName: "OrchardCore.Search",
                pattern: "search/{index?}",
                defaults: new { controller = typeof(SearchController).ControllerName(), action = nameof(SearchController.Search) }
            );
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDeployment<SearchSettingsDeploymentSource, SearchSettingsDeploymentStep, SearchSettingsDeploymentStepDriver>();
        }
    }

    [RequireFeatures("OrchardCore.Liquid")]
    public class LiquidStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<SearchIndexViewModel>();
                o.MemberAccessStrategy.Register<SearchFormViewModel>();
                o.MemberAccessStrategy.Register<SearchResultsViewModel>();
            });
        }
    }
}
