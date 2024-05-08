using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Navigation;
using OrchardCore.Search.AzureAI.Drivers;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI;

public class AdminMenu(
    IStringLocalizer<AdminMenu> stringLocalizer,
    IOptions<AzureAISearchDefaultOptions> azureAISearchSettings) : INavigationProvider
{
    protected readonly IStringLocalizer S = stringLocalizer;
    private readonly AzureAISearchDefaultOptions _azureAISearchSettings = azureAISearchSettings.Value;

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                .AddClass("search")
                .Id("search")
                .Add(S["Indexing"], S["Indexing"].PrefixPosition(), indexing => indexing
                    .Add(S["Azure AI Indices"], S["Azure AI Indices"].PrefixPosition(), indexes => indexes
                        .Action("Index", "Admin", "OrchardCore.Search.AzureAI")
                        .AddClass("azureaiindices")
                        .Id("azureaiindices")
                        .Permission(AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes)
                        .LocalNav()
                    )
                )
            );

        if (!_azureAISearchSettings.DisableUIConfiguration)
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Azure AI Search"], S["Azure AI Search"].PrefixPosition(), azureAISearch => azureAISearch
                        .AddClass("azure-ai-search")
                            .Id("azureaisearch")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = AzureAISearchDefaultSettingsDisplayDriver.GroupId })
                            .Permission(AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes)
                            .LocalNav()
                        )
                    )
                );
        }

        return Task.CompletedTask;
    }
}
