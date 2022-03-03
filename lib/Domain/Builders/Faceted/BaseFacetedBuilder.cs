using Gotenberg.Sharp.API.Client.Domain.Requests;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public abstract class BaseFacetedBuilder<TRequest> where TRequest : RequestBase
{
    protected TRequest Request { get; set; }
}