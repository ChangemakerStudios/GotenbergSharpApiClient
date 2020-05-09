using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Builders.Facets;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class PaperSizeExtensions
    {
        static readonly IEnumerable<(PaperSizes Size, double Width, double Height)> PaperSizer = new[]
        {
            (PaperSizes.A3, Width: 11.7, Height: 16.5),
            (PaperSizes.A4, Width: 8.27, Height: 11.7),
            (PaperSizes.A5, Width: 5.8, Height: 8.2),
            (PaperSizes.A6, Width: 4.1, Height: 5.8),
            (PaperSizes.Letter, Width: 8.5, Height: 11),
            (PaperSizes.Legal, Width: 8.5, Height: 14),
            (PaperSizes.Tabloid, Width: 11, Height: 17)
        };

        // ReSharper disable once FlagArgument
        public static (PaperSizes Size, double Width, double Height) ToSelectedSize(this PaperSizes selectedSize)
        {
            if (!Enum.IsDefined(typeof(PaperSizes), selectedSize))
                throw new InvalidEnumArgumentException(nameof(selectedSize), (int) selectedSize, typeof(PaperSizes));
            if (selectedSize == PaperSizes.None)
                throw new ArgumentOutOfRangeException(nameof(selectedSize));

            return PaperSizer.First(s => s.Size == selectedSize);
        }
    }
}