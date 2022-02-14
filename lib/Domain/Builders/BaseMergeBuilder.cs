using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

public abstract class BaseMergeBuilder<TBuilder, TRequest> : BaseBuilder<TBuilder, TRequest>
    where TRequest : RequestBase
    where TBuilder : BaseMergeBuilder<TBuilder, TRequest>
{
    protected readonly List<Task> AsyncTasks = new List<Task>();


    /// <summary>
    /// This tells gotenberg to have OfficeLibre perform the conversion.
    /// If you set <see cref="MergeOfficeRequest.UseNativePdfFormat"/> to true
    /// then gotenberg will hand the work off to unoconv to do the work
    /// </summary>
    [PublicAPI]
    public TBuilder SetPdfFormat(PdfFormats format)
    {
        this.Request.Format = format;
        return (TBuilder)this;
    }

    [PublicAPI]
    public TBuilder WithAssets(Action<AssetBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        action(new AssetBuilder(this.Request));
        return (TBuilder)this;
    }

    [PublicAPI]
    public TBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));
        this.AsyncTasks.Add(asyncAction(new AssetBuilder(this.Request)));
        return (TBuilder)this;
    }
}
