using System.Text.Json.Serialization;
using Lucene.Net.Analysis.Standard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Recipes;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene.Deployment;
using OrchardCore.Search.Lucene.Drivers;
using OrchardCore.Search.Lucene.Handler;
using OrchardCore.Search.Lucene.Handlers;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Recipes;
using OrchardCore.Search.Lucene.Services;
using OrchardCore.Search.Lucene.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDataMigration<Migrations>();
            services.AddSingleton<LuceneIndexingState>();
            services.AddSingleton<LuceneIndexSettingsService>();
            services.AddSingleton<LuceneIndexManager>();
            services.AddSingleton<LuceneAnalyzerManager>();
            services.AddScoped<LuceneIndexingService>();
            services.AddScoped<IModularTenantEvents, LuceneIndexInitializerService>();
            services.AddScoped<ILuceneSearchQueryService, LuceneSearchQueryService>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.Configure<LuceneOptions>(o =>
                o.Analyzers.Add(new LuceneAnalyzer(LuceneSettings.StandardAnalyzer,
                    new StandardAnalyzer(LuceneSettings.DefaultVersion))));

            services.AddScoped<IDisplayDriver<Query>, LuceneQueryDisplayDriver>();

            services.AddScoped<IContentHandler, LuceneIndexingContentHandler>();
            services.AddLuceneQueries();

            // LuceneQuerySource is registered for both the Queries module and local usage.
            services.AddScoped<IQuerySource, LuceneQuerySource>();
            services.AddScoped<LuceneQuerySource>();
            services.AddRecipeExecutionStep<LuceneIndexStep>();
            services.AddRecipeExecutionStep<LuceneIndexRebuildStep>();
            services.AddRecipeExecutionStep<LuceneIndexResetStep>();
            services.AddScoped<IAuthorizationHandler, LuceneAuthorizationHandler>();

            // Allows to serialize 'LuceneQuery' from its base type.
            services.AddJsonDerivedTypeInfo<LuceneQuery, Query>();
        }
    }

    [RequireFeatures("OrchardCore.Search")]
    public class SearchStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISearchService, LuceneSearchService>();
            services.AddScoped<IDisplayDriver<ISite>, LuceneSettingsDisplayDriver>();
            services.AddScoped<IAuthorizationHandler, LuceneAuthorizationHandler>();
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDeployment<LuceneIndexDeploymentSource, LuceneIndexDeploymentStep, LuceneIndexDeploymentStepDriver>();
            services.AddDeployment<LuceneSettingsDeploymentSource, LuceneSettingsDeploymentStep, LuceneSettingsDeploymentStepDriver>();
            services.AddDeployment<LuceneIndexRebuildDeploymentSource, LuceneIndexRebuildDeploymentStep, LuceneIndexRebuildDeploymentStepDriver>();
            services.AddDeployment<LuceneIndexResetDeploymentSource, LuceneIndexResetDeploymentStep, LuceneIndexResetDeploymentStepDriver>();
        }
    }

    [Feature("OrchardCore.Search.Lucene.Worker")]
    public class LuceneWorkerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
        }
    }

    [Feature("OrchardCore.Search.Lucene.ContentPicker")]
    public class LuceneContentPickerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPickerResultProvider, LuceneContentPickerResultProvider>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldLuceneEditorSettingsDriver>();
            services.AddShapeAttributes<LuceneContentPickerShapeProvider>();
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
