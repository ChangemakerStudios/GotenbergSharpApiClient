//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
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
using System.Reflection;
using Gotenberg.Sharp.API.Client.Domain.Dimensions;

namespace Gotenberg.Sharp.API.Client.Extensions;

internal static class EnumExtensions
{
    private static readonly IEnumerable<(Margins MarginType, (Dimension Left, Dimension Right, Dimension Top, Dimension Bottom) Value)> MarginSizer =
    [
        (Margins.None, (Left: Dimension.FromInches(0.0), Right: Dimension.FromInches(0.0), Top: Dimension.FromInches(0.0), Bottom: Dimension.FromInches(0.0))),
        (Margins.Normal, (Left: Dimension.FromInches(1.0), Right: Dimension.FromInches(1.0), Top: Dimension.FromInches(1.0), Bottom: Dimension.FromInches(1.0))),
        (Margins.Large, (Left: Dimension.FromInches(2.0), Right: Dimension.FromInches(2.0), Top: Dimension.FromInches(2.0), Bottom: Dimension.FromInches(2.0)))
    ];

    private static readonly IEnumerable<(PaperSizes Size, (Dimension Width, Dimension Height) Value)> PaperSizer =
    [
        (PaperSizes.A3, (Width: Dimension.FromInches(11.7), Height: Dimension.FromInches(16.5))),
        (PaperSizes.A4, (Width: Dimension.FromInches(8.27), Height: Dimension.FromInches(11.7))),
        (PaperSizes.A5, (Width: Dimension.FromInches(5.8), Height: Dimension.FromInches(8.2))),
        (PaperSizes.A6, (Width: Dimension.FromInches(4.1), Height: Dimension.FromInches(5.8))),
        (PaperSizes.Letter, (Width: Dimension.FromInches(8.5), Height: Dimension.FromInches(11.0))),
        (PaperSizes.Legal, (Width: Dimension.FromInches(8.5), Height: Dimension.FromInches(14.0))),
        (PaperSizes.Tabloid, (Width: Dimension.FromInches(11.0), Height: Dimension.FromInches(17.0)))
    ];

    internal static string ToFormDataValue(this PdfFormats format)
    {
        return format == default
            ? "PDF/A-1a"
            : $"PDF/A-{format.ToString().Substring(1, 2)}";
    }

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

    public static string GetDescription(this Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString())!;
        DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }
}