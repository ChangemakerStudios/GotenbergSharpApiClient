using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public class ExtraScriptTags : BaseExtraResourceItems<ExtraScriptTagItem>
{
    public ExtraScriptTags() : base(Constants.Gotenberg.FormFieldNames.HtmlConvertBehaviors.UrlConvert.ExtraScriptTags)
    {
    }
}