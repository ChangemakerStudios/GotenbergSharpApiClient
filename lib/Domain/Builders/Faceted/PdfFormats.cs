using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

[PublicAPI]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum PdfFormats
{
    [UsedImplicitly]
    None = 0,
    A1a = 1,
    A1b = 2,
    A2a = 3,
    A2b = 4,
    A2u =5,
    A3a =6,
    A3b = 7,
    A3u = 8
}