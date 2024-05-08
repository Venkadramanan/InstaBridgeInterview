using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nest;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Providers;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Drivers;
using OrchardCore.Search.Elasticsearch.Services;
using OrchardCore.Search.Lucene.Handler;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search.Elasticsearch
{
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;

        public Startup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<ElasticConnectionOptions>, ElasticConnectionOptionsConfigurations>();

            services.AddSingleton<IElasticClient>((sp) =>
            {
                var options = sp.GetRequiredService<IOptions<ElasticConnectionOptions>>().Value;

                return new ElasticClient(options.GetConnectionSettings() ?? new ConnectionSettings());
            });

            services.Configure<ElasticsearchOptions>(o =>
            {
                var configuration = _shellConfiguration.GetSection(ElasticConnectionOptionsConfigurations.ConfigSectionName);

                o.IndexPrefix = configuration.GetValue<string>(nameof(o.IndexPrefix));

                var jsonNode = configuration.GetSection(nameof(o.Analyzers)).AsJsonNode();
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonNode);

                var analyzersObject = JsonObject.Create(jsonElement, new JsonNodeOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (analyzersObject != null)
                {
                    o.IndexPrefix = configuration.GetValue<string>(nameof(o.IndexPrefix));

                    if (jsonNode is JsonObject jAnalyzers)
                    {
                        foreach (var analyzer in jAnalyzers)
                        {
                            if (analyzer.Value is not JsonObject jAnalyzer)
                            {
                                continue;
                            }

                            o.Analyzers.Add(analyzer.Key, jAnalyzer);
                        }
                    }
                }

                if (o.Analyzers.Count == 0)
                {
                    // When no analyzers are configured, we'll define a default analyzer.
                    o.Analyzers.Add(ElasticsearchConstants.DefaultAnalyzer, new JsonObject
                    {
                        ["type"] = "standard",
                    });
                }
            });

            services.AddElasticServices();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<Query>, ElasticQueryDisplayDriver>();
        }
    }

    [RequireFeatures("OrchardCore.Search")]
    public class SearchStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISearchService, ElasticsearchService>();
            services.AddScoped<IDisplayDriver<ISite>, ElasticSettingsDisplayDriver>();
            services.AddScoped<IAuthorizationHandler, ElasticsearchAuthorizationHandler>();
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDeployment<ElasticIndexDeploymentSource, ElasticIndexDeploymentStep, ElasticIndexDeploymentStepDriver>();
            services.AddDeployment<ElasticSettingsDeploymentSource, ElasticSettingsDeploymentStep, ElasticSettingsDeploymentStepDriver>();
            services.AddDeployment<ElasticIndexRebuildDeploymentSource, ElasticIndexRebuildDeploymentStep, ElasticIndexRebuildDeploymentStepDriver>();
            services.AddDeployment<ElasticIndexResetDeploymentSource, ElasticIndexResetDeploymentStep, ElasticIndexResetDeploymentStepDriver>();
        }
    }

    [Feature("OrchardCore.Search.Elasticsearch.Worker")]
    public class ElasticWorkerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
        }
    }

    [Feature("OrchardCore.Search.Elasticsearch.ContentPicker")]
    public class ElasticContentPickerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPickerResultProvider, ElasticContentPickerResultProvider>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldElasticEditorSettingsDriver>();
            services.AddShapeAttributes<ElasticContentPickerShapeProvider>();
        }
    }

    [RequireFeatures("OrchardCore.ContentTypes")]
    public class ContentTypesStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();
        }
    }
}
