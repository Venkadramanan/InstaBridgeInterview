using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Drivers
{
    public class AdminSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, AdminSettings>
    {
        public const string GroupId = "admin";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public AdminSiteSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(AdminSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, PermissionsAdminSettings.ManageAdminSettings))
            {
                return null;
            }

            return Initialize<AdminSettingsViewModel>("AdminSettings_Edit", model =>
            {
                model.DisplayThemeToggler = settings.DisplayThemeToggler;
                model.DisplayMenuFilter = settings.DisplayMenuFilter;
                model.DisplayNewMenu = settings.DisplayNewMenu;
                model.DisplayTitlesInTopbar = settings.DisplayTitlesInTopbar;
            }).Location("Content:3").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(AdminSettings settings, UpdateEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, PermissionsAdminSettings.ManageAdminSettings))
            {
                return null;
            }

            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                var model = new AdminSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.DisplayThemeToggler = model.DisplayThemeToggler;
                settings.DisplayMenuFilter = model.DisplayMenuFilter;
                settings.DisplayNewMenu = model.DisplayNewMenu;
                settings.DisplayTitlesInTopbar = model.DisplayTitlesInTopbar;
            }

            return await EditAsync(settings, context);
        }
    }
}
