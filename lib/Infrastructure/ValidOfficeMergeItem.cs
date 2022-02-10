using System.Collections.Generic;

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

public class ValidOfficeMergeItem
{
    public string MediaType { get; set; }
    public KeyValuePair<string, ContentItem> Asset { get; set; }
}