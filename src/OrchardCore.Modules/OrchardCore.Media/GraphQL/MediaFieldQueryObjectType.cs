using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Media.Fields;

namespace OrchardCore.Media.GraphQL
{
    public class MediaFieldQueryObjectType : ObjectGraphType<MediaField>
    {
        public MediaFieldQueryObjectType()
        {
            Name = nameof(MediaField);

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("paths")
                .Description("the media paths")
                .PagingArguments()
                .Resolve(x =>
                {
                    if (x.Source?.Paths is null)
                    {
                        return Array.Empty<string>();
                    }
                    return x.Page(x.Source.Paths);
                });

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("fileNames")
                .Description("the media fileNames")
                .PagingArguments()
                .Resolve(x =>
                {
                    var fileNames = x.Page(x.Source.GetAttachedFileNames());
                    if (fileNames is null)
                    {
                        return Array.Empty<string>();
                    }
                    return fileNames;
                });

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("urls")
                .Description("the absolute urls of the media items")
                .PagingArguments()
                .Resolve(x =>
                {
                    if (x.Source?.Paths is null)
                    {
                        return Array.Empty<string>();
                    }
                    var paths = x.Page(x.Source.Paths);
                    var mediaFileStore = x.RequestServices.GetService<IMediaFileStore>();
                    return paths.Select(p => mediaFileStore.MapPathToPublicUrl(p));
                });

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("mediatexts")
                .Description("the media texts")
                .PagingArguments()
                .Resolve(x =>
                {
                    if (x.Source?.MediaTexts is null)
                    {
                        return Array.Empty<string>();
                    }
                    return x.Page(x.Source.MediaTexts);
                });
        }
    }
}
