using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users
{
    public class AdminMenu : INavigationProvider
    {
        private static readonly RouteValueDictionary _routeValues = new()
        {
            { "area", "OrchardCore.Settings" },
            { "groupId", LoginSettingsDisplayDriver.GroupId },
        };

        protected readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!NavigationHelper.IsAdminMenu(name))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Security"], NavigationConstants.AdminMenuSecurityPosition, security => security
                    .AddClass("security")
                    .Id("security")
                    .Add(S["Users"], S["Users"].PrefixPosition(), users => users
                        .AddClass("users")
                        .Id("users")
                        .Action("Index", "Admin", "OrchardCore.Users")
                        .Permission(CommonPermissions.ListUsers)
                        .Resource(new User())
                        .LocalNav()
                    )
                    .Add(S["Settings"], settings => settings
                        .Add(S["User Login"], S["User Login"].PrefixPosition(), login => login
                            .Permission(CommonPermissions.ManageUsers)
                            .Action("Index", "Admin", _routeValues)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Users.ChangeEmail")]
    public class ChangeEmailAdminMenu : INavigationProvider
    {
        private static readonly RouteValueDictionary _routeValues = new()
        {
            { "area", "OrchardCore.Settings" },
            { "groupId", ChangeEmailSettingsDisplayDriver.GroupId },
        };

        protected readonly IStringLocalizer S;

        public ChangeEmailAdminMenu(IStringLocalizer<ChangeEmailAdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!NavigationHelper.IsAdminMenu(name))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Security"], security => security
                    .Add(S["Settings"], settings => settings
                        .Add(S["User Change email"], S["User Change email"].PrefixPosition(), email => email
                            .Permission(CommonPermissions.ManageUsers)
                            .Action("Index", "Admin", _routeValues)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }

    [Feature(UserConstants.Features.UserRegistration)]
    public class RegistrationAdminMenu : INavigationProvider
    {
        private static readonly RouteValueDictionary _routeValues = new()
        {
            { "area", "OrchardCore.Settings" },
            { "groupId", RegistrationSettingsDisplayDriver.GroupId },
        };

        protected readonly IStringLocalizer S;

        public RegistrationAdminMenu(IStringLocalizer<RegistrationAdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!NavigationHelper.IsAdminMenu(name))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Security"], security => security
                    .Add(S["Settings"], settings => settings
                        .Add(S["User Registration"], S["User Registration"].PrefixPosition(), registration => registration
                            .Permission(CommonPermissions.ManageUsers)
                            .Action("Index", "Admin", _routeValues)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }

    [Feature(UserConstants.Features.ResetPassword)]
    public class ResetPasswordAdminMenu : INavigationProvider
    {
        private static readonly RouteValueDictionary _routeValues = new()
        {
            { "area", "OrchardCore.Settings" },
            { "groupId", ResetPasswordSettingsDisplayDriver.GroupId },
        };

        protected readonly IStringLocalizer S;

        public ResetPasswordAdminMenu(IStringLocalizer<ResetPasswordAdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!NavigationHelper.IsAdminMenu(name))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Security"], security => security
                    .Add(S["Settings"], settings => settings
                        .Add(S["User Reset password"], S["User Reset password"].PrefixPosition(), password => password
                            .Permission(CommonPermissions.ManageUsers)
                            .Action("Index", "Admin", _routeValues)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
