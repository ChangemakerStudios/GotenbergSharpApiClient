using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public class ExtraUrlResources : IConvertToHttpContent 
{
    public List<ExtraUrlResourceItem> Items { get; set; } = new();

    public IEnumerable<HttpContent> ToHttpContent()
    {
        foreach (var g in Items.GroupBy(i => i.FormDataFieldName))
        {
            var groupValue = string.Join(", ", g.Select(gi=> gi.ToJson()));
            yield return RequestBase.CreateFormDataItem($"[{groupValue}]", g.Key);
        }
    }
}