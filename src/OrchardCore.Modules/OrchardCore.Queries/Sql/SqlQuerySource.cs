using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dapper;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using OrchardCore.Json;
using OrchardCore.Liquid;
using YesSql;

namespace OrchardCore.Queries.Sql
{
    public class SqlQuerySource : IQuerySource
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IDbConnectionAccessor _dbConnectionAccessor;
        private readonly ISession _session;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly TemplateOptions _templateOptions;

        public SqlQuerySource(
            ILiquidTemplateManager liquidTemplateManager,
            IDbConnectionAccessor dbConnectionAccessor,
            ISession session,
            IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
            IOptions<TemplateOptions> templateOptions)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _dbConnectionAccessor = dbConnectionAccessor;
            _session = session;
            _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
            _templateOptions = templateOptions.Value;
        }

        public string Name => "Sql";

        public Query Create()
        {
            return new SqlQuery();
        }

        public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var sqlQuery = query as SqlQuery;
            var sqlQueryResults = new SQLQueryResults();

            var tokenizedQuery = await _liquidTemplateManager.RenderStringAsync(sqlQuery.Template, NullEncoder.Default,
                parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

            var dialect = _session.Store.Configuration.SqlDialect;

            if (!SqlParser.TryParse(tokenizedQuery, _session.Store.Configuration.Schema, dialect, _session.Store.Configuration.TablePrefix, parameters, out var rawQuery, out var messages))
            {
                sqlQueryResults.Items = Array.Empty<object>();

                return sqlQueryResults;
            }

            await using var connection = _dbConnectionAccessor.CreateConnection();

            await connection.OpenAsync();

            if (sqlQuery.ReturnDocuments)
            {
                IEnumerable<long> documentIds;

                using var transaction = await connection.BeginTransactionAsync(_session.Store.Configuration.IsolationLevel);
                documentIds = await connection.QueryAsync<long>(rawQuery, parameters, transaction);

                sqlQueryResults.Items = await _session.GetAsync<ContentItem>(documentIds.ToArray());

                return sqlQueryResults;
            }
            else
            {
                IEnumerable<dynamic> queryResults;

                using var transaction = await connection.BeginTransactionAsync(_session.Store.Configuration.IsolationLevel);
                queryResults = await connection.QueryAsync(rawQuery, parameters, transaction);

                var results = new List<JsonObject>();
                foreach (var document in queryResults)
                {
                    results.Add(JObject.FromObject(document, _jsonSerializerOptions));
                }

                sqlQueryResults.Items = results;
                return sqlQueryResults;
            }
        }
    }
}
