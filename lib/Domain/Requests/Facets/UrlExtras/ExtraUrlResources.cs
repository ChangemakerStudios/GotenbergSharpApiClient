
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

[PublicAPI]
public class ExtraUrlResources : IConvertToHttpContent
{
    readonly UrlRequest _request;

    public ExtraUrlResources(UrlRequest request)
    {
        _request = request;
        LinkTags = new ExtraLinkTags();
        ScriptTags = new ExtraScriptTags();
    }

    public ExtraLinkTags LinkTags { get; set; }

    public ExtraScriptTags ScriptTags { get; set; }

    public IEnumerable<HttpContent> ToHttpContent()
    {
        return LinkTags.IfNullEmptyContent()
            .Concat(ScriptTags.IfNullEmptyContent())
            .Concat(_request.Assets.IfNullEmptyContent());
    }
}