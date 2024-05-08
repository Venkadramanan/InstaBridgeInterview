using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Seo;

public class SeoPermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageSeoSettings = SeoConstants.ManageSeoSettings;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSeoSettings,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
