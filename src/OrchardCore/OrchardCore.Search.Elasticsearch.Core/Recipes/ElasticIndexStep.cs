using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes
{
    /// <summary>
    /// This recipe step creates a Elasticsearch index.
    /// </summary>
    public class ElasticIndexStep : IRecipeStepHandler
    {
        private readonly ElasticIndexingService _elasticIndexingService;
        private readonly ElasticIndexManager _elasticIndexManager;

        public ElasticIndexStep(
            ElasticIndexingService elasticIndexingService,
            ElasticIndexManager elasticIndexManager
            )
        {
            _elasticIndexManager = elasticIndexManager;
            _elasticIndexingService = elasticIndexingService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "ElasticIndexSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indices = context.Step["Indices"];
            if (indices is JsonArray jsonArray)
            {
                foreach (var index in jsonArray)
                {
                    var elasticIndexSettings = index.ToObject<Dictionary<string, ElasticIndexSettings>>().FirstOrDefault();

                    if (!await _elasticIndexManager.ExistsAsync(elasticIndexSettings.Key))
                    {
                        elasticIndexSettings.Value.IndexName = elasticIndexSettings.Key;
                        await _elasticIndexingService.CreateIndexAsync(elasticIndexSettings.Value);
                    }
                }
            }
        }
    }
}
