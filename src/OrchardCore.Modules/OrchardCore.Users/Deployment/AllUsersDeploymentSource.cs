using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Deployment;

public class AllUsersDeploymentSource : IDeploymentSource
{
    private readonly ISession _session;

    public AllUsersDeploymentSource(ISession session)
    {
        _session = session;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AllUsersDeploymentStep)
        {
            return;
        }

        var allUsers = await _session.Query<User>().ListAsync();
        var users = new JsonArray();

        foreach (var user in allUsers)
        {
            users.Add(JObject.FromObject(
                new UsersStepUserModel
                {
                    UserName = user.UserName,
                    UserId = user.UserId,
                    Id = user.Id,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PasswordHash = user.PasswordHash,
                    IsEnabled = user.IsEnabled,
                    NormalizedEmail = user.NormalizedEmail,
                    NormalizedUserName = user.NormalizedUserName,
                    SecurityStamp = user.SecurityStamp,
                    ResetToken = user.ResetToken,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    IsLockoutEnabled = user.IsLockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount,
                    RoleNames = user.RoleNames,
                }));
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Users",
            ["Users"] = users,
        });
    }
}
