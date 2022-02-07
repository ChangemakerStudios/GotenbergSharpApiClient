using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public sealed class ExtraLinkTags: BaseExtraResourceItems<ExtraLinkTagItem>
{
    public ExtraLinkTags() : base(Constants.Gotenberg.FormFieldNames.HtmlConvertBehaviors.UrlConvert.ExtraLinkTags)
    {
    }
}

 