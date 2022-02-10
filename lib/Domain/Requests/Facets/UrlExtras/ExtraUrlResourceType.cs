using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public enum ExtraUrlResourceType
{
    [UsedImplicitly]
    None = 0,
    LinkTag = 1,
    ScriptTag = 2
}