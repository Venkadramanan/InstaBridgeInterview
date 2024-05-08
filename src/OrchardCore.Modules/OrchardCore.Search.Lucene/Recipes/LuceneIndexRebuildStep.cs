using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes
{
    /// <summary>
    /// This recipe step rebuilds a Lucene index.
    /// </summary>
    public class LuceneIndexRebuildStep : IRecipeStepHandler
    {
        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "lucene-index-rebuild", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<LuceneIndexRebuildStepModel>();

            if (model.IncludeAll || model.Indices.Length > 0)
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("lucene-index-rebuild", async (scope) =>
                {
                    var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                    var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();

                    var indices = model.IncludeAll ? (await luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray() : model.Indices;

                    foreach (var indexName in indices)
                    {
                        var luceneIndexSettings = await luceneIndexSettingsService.GetSettingsAsync(indexName);
                        if (luceneIndexSettings != null)
                        {
                            await luceneIndexingService.RebuildIndexAsync(indexName);
                            await luceneIndexingService.ProcessContentItemsAsync(indexName);
                        }
                    }
                });
            }
        }

        private sealed class LuceneIndexRebuildStepModel
        {
            public bool IncludeAll { get; set; } = false;
            public string[] Indices { get; set; } = [];
        }
    }
}
