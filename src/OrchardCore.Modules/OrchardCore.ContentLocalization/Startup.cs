using System;
using System.Globalization;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Admin.Models;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.ContentLocalization.Indexing;
using OrchardCore.ContentLocalization.Liquid;
using OrchardCore.ContentLocalization.Security;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentLocalization.Sitemaps;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Builders;

namespace OrchardCore.ContentLocalization
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<LocalizationPartViewModel>();
                o.MemberAccessStrategy.Register<CultureInfo>();
            })
            .AddLiquidFilter<ContentLocalizationFilter>("localization_set");

            services.AddScoped<IContentPartIndexHandler, LocalizationPartIndexHandler>();
            services.AddSingleton<ILocalizationEntries, LocalizationEntries>();
            services.AddContentLocalization();

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IAuthorizationHandler, LocalizeContentAuthorizationHandler>();

            services.AddScoped<IContentsAdminListFilter, LocalizationPartContentsAdminListFilter>();
            services.AddTransient<IContentsAdminListFilterProvider, LocalizationPartContentsAdminListFilterProvider>();
            services.AddScoped<IDisplayDriver<ContentOptionsViewModel>, LocalizationContentsAdminListDisplayDriver>();
        }
    }

    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class ContentPickerStartup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;
        public ContentPickerStartup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<Navbar>, ContentCulturePickerNavbarDisplayDriver>();
            services.AddLiquidFilter<SwitchCultureUrlFilter>("switch_culture_url");
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IContentCulturePickerService, ContentCulturePickerService>();
            services.AddScoped<IDisplayDriver<ISite>, ContentCulturePickerSettingsDriver>();
            services.AddScoped<IDisplayDriver<ISite>, ContentRequestCultureProviderSettingsDriver>();
            services.Configure<RequestLocalizationOptions>(options => options.AddInitialRequestCultureProvider(new ContentRequestCultureProvider()));
            services.Configure<CulturePickerOptions>(_shellConfiguration.GetSection("OrchardCore_ContentLocalization_CulturePickerOptions"));
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
               name: "RedirectToLocalizedContent",
               areaName: "OrchardCore.ContentLocalization",
               pattern: "RedirectToLocalizedContent",
               defaults: new { controller = "ContentCulturePicker", action = "RedirectToLocalizedContent" }
           );
        }
    }

    [Feature("OrchardCore.ContentLocalization.Sitemaps")]
    public class SitemapsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISitemapContentItemExtendedMetadataProvider, SitemapUrlHrefLangExtendedMetadataProvider>();
            services.Replace(ServiceDescriptor.Scoped<IContentItemsQueryProvider, LocalizedContentItemsQueryProvider>());
        }
    }
}
