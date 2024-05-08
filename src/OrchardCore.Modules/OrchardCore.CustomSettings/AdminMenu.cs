using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.CustomSettings
{
    public class AdminMenu : INavigationProvider
    {
        private readonly CustomSettingsService _customSettingsService;
        protected readonly IStringLocalizer S;
        private static readonly ConcurrentDictionary<string, RouteValueDictionary> _routeValues = [];

        public AdminMenu(
            IStringLocalizer<AdminMenu> localizer,
            CustomSettingsService customSettingsService)
        {
            S = localizer;
            _customSettingsService = customSettingsService;
        }

        public async Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!NavigationHelper.IsAdminMenu(name))
            {
                return;
            }

            foreach (var type in await _customSettingsService.GetAllSettingsTypesAsync())
            {
                if (!_routeValues.TryGetValue(type.Name, out var routeValues))
                {
                    routeValues = new RouteValueDictionary()
                    {
                         { "area", "OrchardCore.Settings" },
                         { "groupId", type.Name },
                    };

                    _routeValues[type.Name] = routeValues;
                }

                var htmlName = type.Name.HtmlClassify();

                builder
                    .Add(S["Configuration"], configuration => configuration
                        .Add(S["Settings"], settings => settings
                            .Add(new LocalizedString(type.DisplayName, type.DisplayName), type.DisplayName.PrefixPosition(), layers => layers
                                .Action("Index", "Admin", routeValues)
                                .AddClass(htmlName)
                                .Id(htmlName)
                                .Permission(Permissions.CreatePermissionForType(type))
                                .Resource(type.Name)
                                .LocalNav()
                            )
                        )
                    );
            }
        }
    }
}
