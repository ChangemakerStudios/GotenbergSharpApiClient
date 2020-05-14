using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

// ReSharper disable FlagArgument

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class EnumExtensions
    {
        static readonly IEnumerable<(Margins MarginType,
            (double Left, double Right, double Top, double Bottom) Value)> MarginSizer = new[]
        {
            (Margins.None, (Left: 0.0, Right: 0.0, Top: 0.0, Bottom: 0.0)),
            (Margins.Normal, (Left: 1.0, Right: 1.0, Top: 1.0, Bottom: 1.0)),
            (Margins.Large, (Left: 2.0, Right: 2.0, Top: 2.0, Bottom: 2.0))
        };

        static readonly IEnumerable<(PaperSizes Size, (double Width, double Height) Value)> PaperSizer = new[]
        {
            (PaperSizes.A3, (Width: 11.7, Height: 16.5)),
            (PaperSizes.A4, (Width: 8.27, Height: 11.7)),
            (PaperSizes.A5, (Width: 5.8, Height: 8.2)),
            (PaperSizes.A6, (Width: 4.1, Height: 5.8)),
            (PaperSizes.Letter, (Width: 8.5, Height: 11.0)),
            (PaperSizes.Legal, (Width: 8.5, Height: 14.0)),
            (PaperSizes.Tabloid, (Width: 11.0, Height: 17.0))
        };

        public static (double Width, double Height) ToSelectedSize(this PaperSizes selectedSize)
        {
            if (!Enum.IsDefined(typeof(PaperSizes), selectedSize))
                throw new InvalidEnumArgumentException(nameof(selectedSize), (int) selectedSize, typeof(PaperSizes));
            if (selectedSize == PaperSizes.None)
                throw new InvalidOperationException(nameof(selectedSize));

            return PaperSizer.First(s => s.Size == selectedSize).Value;
        }

        public static (double Left, double Right, double Top, double Bottom) ToSelectedMargins(this Margins selected)
        {
            if (!Enum.IsDefined(typeof(Margins), selected))
                throw new InvalidEnumArgumentException(nameof(selected), (int) selected, typeof(PaperSizes));

            return MarginSizer.First(m => m.MarginType == selected).Value;
        }
    }
}