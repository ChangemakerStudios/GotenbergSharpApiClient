using JetBrains.Annotations;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

[PublicAPI]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum PdfFormats
{
    [UsedImplicitly]
    None = 0,

    [Description("PDF/A-1a")]
    A1a = 1,

    [Description("PDF/A-1b")]
    A1b = 2,
    
    [Description("PDF/A-2a")]
    A2a = 3,

    [Description("PDF/A-2b")]
    A2b = 4,

    [Description("PDF/A-2u")]
    A2u =5,

    [Description("PDF/A-3a")]
    A3a =6,

    [Description("PDF/A-3b")]
    A3b = 7,

    [Description("PDF/A-3u")]
    A3u = 8
}