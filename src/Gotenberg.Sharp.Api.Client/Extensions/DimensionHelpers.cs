// Copyright 2019-2025 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System.ComponentModel;
using Gotenberg.Sharp.API.Client.Domain.Dimensions;
using Gotenberg.Sharp.API.Client.Domain.Pages;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class DimensionHelpers
    {
        private static readonly IEnumerable<(Margins MarginType, (Dimension Left, Dimension Right, Dimension Top, Dimension Bottom) Value)> MarginSizer =
        [
            (Margins.None, (Left: Inches(0.0), Right: Inches(0.0), Top: Inches(0.0), Bottom: Inches(0.0))),
            (Margins.Default, (Left: Inches(0.39), Right: Inches(0.39), Top: Inches(0.39), Bottom: Inches(0.39))),
            (Margins.Normal, (Left: Inches(1.0), Right: Inches(1.0), Top: Inches(1.0), Bottom: Inches(1.0))),
            (Margins.Large, (Left: Inches(2.0), Right: Inches(2.0), Top: Inches(2.0), Bottom: Inches(2.0)))
        ];

        private static readonly IEnumerable<(PaperSizes Size, (Dimension Width, Dimension Height) Value)> PaperSizer =
        [
            // The ISO A series is defined in millimetres; rounded inch values (8.27 x 11.7) are not exactly A4.
            (PaperSizes.A3, (Width: Dimension.FromMillimeters(297), Height: Dimension.FromMillimeters(420))),
            (PaperSizes.A4, (Width: Dimension.FromMillimeters(210), Height: Dimension.FromMillimeters(297))),
            (PaperSizes.A5, (Width: Dimension.FromMillimeters(148), Height: Dimension.FromMillimeters(210))),
            (PaperSizes.A6, (Width: Dimension.FromMillimeters(105), Height: Dimension.FromMillimeters(148))),
            (PaperSizes.Letter, (Width: Inches(8.5), Height: Inches(11.0))),
            (PaperSizes.Legal, (Width: Inches(8.5), Height: Inches(14.0))),
            (PaperSizes.Tabloid, (Width: Inches(11.0), Height: Inches(17.0))),
            (PaperSizes.D, (Width: Inches(22.0), Height: Inches(34.0))),
            (PaperSizes.E, (Width: Inches(34.0), Height: Inches(44.0)))
        ];

        private static Dimension Inches(double value) => Dimension.FromInches(value);

        internal static (Dimension Width, Dimension Height) ToSelectedSize(this PaperSizes selectedSize)
        {
            if (!Enum.IsDefined(typeof(PaperSizes), selectedSize))
                throw new InvalidEnumArgumentException(
                    nameof(selectedSize),
                    (int)selectedSize,
                    typeof(PaperSizes));

            if (selectedSize == default)
                throw new InvalidOperationException(nameof(selectedSize));

            return PaperSizer.First(s => s.Size == selectedSize).Value;
        }

        internal static (Dimension Left, Dimension Right, Dimension Top, Dimension Bottom) ToSelectedMargins(
            this Margins selected)
        {
            if (!Enum.IsDefined(typeof(Margins), selected))
                throw new InvalidEnumArgumentException(
                    nameof(selected),
                    (int)selected,
                    typeof(PaperSizes));

            return MarginSizer.First(m => m.MarginType == selected).Value;
        }
    }
}