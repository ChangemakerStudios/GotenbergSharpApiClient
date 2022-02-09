using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public abstract class BaseExtraResourceItems<TResourceItem> : IConvertToHttpContent 
    where TResourceItem : BaseExtraResourceItem
{
    readonly string _formDataFieldName;

    protected BaseExtraResourceItems(string formDataFieldName)
    {
        _formDataFieldName = formDataFieldName;
    }

    public List<TResourceItem> Items { get; set; } = new();

    public IEnumerable<HttpContent> ToHttpContent()
    {
        var value = string.Join(", ", Items.Select(i => i.ToJson()));
      
        return new HttpContent[]
        {
            RequestBase.CreateFormDataItem($"[{value}]", _formDataFieldName)
        };
    }
}