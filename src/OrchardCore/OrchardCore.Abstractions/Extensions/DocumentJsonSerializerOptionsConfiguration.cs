using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Extensions;

public sealed class DocumentJsonSerializerOptionsConfiguration : IConfigureOptions<DocumentJsonSerializerOptions>
{
    private readonly JsonDerivedTypesOptions _derivedTypesOptions;

    public DocumentJsonSerializerOptionsConfiguration(IOptions<JsonDerivedTypesOptions> derivedTypesOptions)
    {
        _derivedTypesOptions = derivedTypesOptions.Value;
    }

    public void Configure(DocumentJsonSerializerOptions options)
    {
        options.SerializerOptions.DefaultIgnoreCondition = JOptions.Base.DefaultIgnoreCondition;
        options.SerializerOptions.ReferenceHandler = JOptions.Base.ReferenceHandler;
        options.SerializerOptions.ReadCommentHandling = JOptions.Base.ReadCommentHandling;
        options.SerializerOptions.PropertyNameCaseInsensitive = JOptions.Base.PropertyNameCaseInsensitive;
        options.SerializerOptions.AllowTrailingCommas = JOptions.Base.AllowTrailingCommas;
        options.SerializerOptions.WriteIndented = JOptions.Base.WriteIndented;

        options.SerializerOptions.TypeInfoResolverChain.Add(new PolymorphicJsonTypeInfoResolver(_derivedTypesOptions));
        options.SerializerOptions.Converters.Add(DynamicJsonConverter.Instance);
        options.SerializerOptions.Converters.Add(PathStringJsonConverter.Instance);
    }
}
