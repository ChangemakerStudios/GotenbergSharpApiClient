using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

public sealed class PdfConversionBuilder : BaseBuilder<PdfConversionRequest>
{
    readonly List<Task> _asyncTasks = new List<Task>();

    public PdfConversionBuilder() 
        => this.Request = new PdfConversionRequest();

    protected override PdfConversionRequest Request { get; set; }

    [PublicAPI]
    public PdfConversionBuilder SetFormat(PdfFormats format)
    {
        if (format == default) throw new ArgumentNullException(nameof(format));

        this.Request.ToFormat = format;
        
        return this;
    }

    [PublicAPI]
    public PdfConversionBuilder WithPdfs(Action<AssetBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        action(new AssetBuilder(this.Request));

        return this;
    }

    [PublicAPI]
    public PdfConversionBuilder WithPdfsAsync(Func<AssetBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));
        this._asyncTasks.Add(asyncAction(new AssetBuilder(this.Request)));
        return this;
    }

    [PublicAPI]
    public PdfConversionRequest Build()
    {
        if (_asyncTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);
        if (Request.Count == 0) throw new InvalidOperationException("There are no items to convert");
        return Request;
    }


    [PublicAPI]
    public async Task<PdfConversionRequest> BuildAsync()
    {
        if (_asyncTasks.Any())
        {
            await Task.WhenAll(_asyncTasks).ConfigureAwait(false);
        }

        return Request;
    }
}