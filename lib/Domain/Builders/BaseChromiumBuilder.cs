using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

public abstract class BaseChromiumBuilder<TRequest, TBuilder> : BaseBuilder<TRequest, TBuilder>
    where TRequest : ChromeRequest
    where TBuilder : BaseChromiumBuilder<TRequest, TBuilder>
{
    protected readonly List<Task> AsyncTasks = new List<Task>();

    [PublicAPI]
    public TBuilder WithDimensions(Action<DimensionBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new DimensionBuilder(this.Request));
        return (TBuilder) this;
    }

    [PublicAPI]
    public TBuilder WithDimensions(Dimensions dimensions)
    {
        this.Request.Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
        return (TBuilder) this;
    }

    [PublicAPI]
    public TBuilder WithAssets(Action<AssetBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        action(new AssetBuilder(this.Request));
        return (TBuilder) this;
    }

    [PublicAPI]
    public TBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));
        this.AsyncTasks.Add(asyncAction(new AssetBuilder(this.Request)));
        return (TBuilder) this;
    }

    [PublicAPI]
    public TBuilder SetConversionBehaviors(Action<HtmlConversionBehaviorBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        action(new HtmlConversionBehaviorBuilder(this.Request));
        return (TBuilder) this;
    }

    [PublicAPI]
    public TBuilder SetConversionBehaviors(HtmlConversionBehaviors behaviors)
    {
        this.Request.ConversionBehaviors = behaviors ?? throw new ArgumentNullException(nameof(behaviors));
        return (TBuilder) this;
    }
}
