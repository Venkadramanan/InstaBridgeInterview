using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.GraphQL.Fields
{
    public class ObjectGraphTypeFieldProvider : IContentFieldProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly ConcurrentDictionary<string, IObjectGraphType> _partObjectGraphTypes = new();

        public ObjectGraphTypeFieldProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public FieldType GetField(ContentPartFieldDefinition field, string namedPartTechnicalName, string customFieldName)
        {
            var queryGraphType = GetObjectGraphType(field);

            if (queryGraphType != null)
            {
                return new FieldType
                {
                    Name = customFieldName ?? field.Name,
                    Description = field.FieldDefinition.Name,
                    Type = queryGraphType.GetType(),
                    Resolver = new FuncFieldResolver<ContentElement, ContentElement>(context =>
                    {
                        var typeToResolve = context.FieldDefinition.ResolvedType.GetType().BaseType.GetGenericArguments().First();

                        // Check if part has been collapsed by trying to get the parent part.
                        ContentElement contentPart = context.Source.Get<ContentPart>(field.PartDefinition.Name);

                        // Part is not collapsed, access field directly.
                        contentPart ??= context.Source;

                        var contentField = contentPart?.Get(typeToResolve, field.Name);
                        return contentField;
                    })
                };
            }

            return null;
        }

        private IObjectGraphType GetObjectGraphType(ContentPartFieldDefinition field)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;

            return _partObjectGraphTypes.GetOrAdd(field.FieldDefinition.Name,
                partName => serviceProvider.GetService<IEnumerable<IObjectGraphType>>()?
                    .FirstOrDefault(x => x.GetType().BaseType.GetGenericArguments().First().Name == partName)
                );
        }

        public bool HasField(ContentPartFieldDefinition field) => GetObjectGraphType(field) != null;
    }
}
