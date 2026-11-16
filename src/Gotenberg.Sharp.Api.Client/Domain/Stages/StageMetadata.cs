// Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using StageOptions = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.Options;

namespace Gotenberg.Sharp.API.Client.Domain.Stages;

/// <summary>
/// Turns <see cref="PdfOutput.PdfOutputOptions.MetaData" />, written for Gotenberg 8 with ExifTool tag
/// names, into the options of the Gotenberg 9 <c>set-metadata</c> stage, which writes eight fields of
/// the document information.
/// </summary>
internal static class StageMetadata
{
    // ExifTool matches tag names case-insensitively; CreateDate and ModifyDate are its names of the
    // PDF dates, CreationDate and ModDate those of the document information.
    private static readonly Dictionary<string, string> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Title"] = StageOptions.Title,
        ["Author"] = StageOptions.Author,
        ["Subject"] = StageOptions.Subject,
        ["Keywords"] = StageOptions.Keywords,
        ["Creator"] = StageOptions.Creator,
        ["Producer"] = StageOptions.Producer,
        ["CreationDate"] = StageOptions.CreationDate,
        ["CreateDate"] = StageOptions.CreationDate,
        ["ModDate"] = StageOptions.ModDate,
        ["ModifyDate"] = StageOptions.ModDate
    };

    /// <summary>
    /// The keys Gotenberg 9 writes, for messages.
    /// </summary>
    internal const string KnownKeys =
        "Title, Author, Subject, Keywords, Creator, Producer, CreationDate (or CreateDate) and ModDate (or ModifyDate)";

    // A PDF date (ISO 32000-1, 7.9.4), as Gotenberg 9 validates it: D:YYYYMMDDHHmmSSOHH'mm, every
    // field after the year optional.
    private static readonly Regex PdfDate = new(
        @"^D:\d{4}(\d{2})?(\d{2})?(\d{2})?(\d{2})?(\d{2})?(Z|[+-]\d{2})?(?:'?\d{2}'?)?$",
        RegexOptions.CultureInvariant);

    // An ISO 8601 date (2006-09-18T16:27:50-04:00) or an ExifTool one (2006:09:18 16:27:50-04:00),
    // the time, its seconds and the offset optional. Fractions of a second are dropped: a PDF date has none.
    private static readonly Regex TextDate = new(
        @"^(?<y>\d{4})(?<sep>[-:])(?<mo>\d{2})\k<sep>(?<d>\d{2})(?:[T ](?<h>\d{2}):(?<mi>\d{2})(?::(?<s>\d{2})(?:\.\d+)?)?)?(?<z>Z|[+-]\d{2}(?::?\d{2})?)?$",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// The first key of <paramref name="metadata" /> that is not one of the eight fields, or null.
    /// </summary>
    internal static string? FindUnknownKey(JObject metadata) =>
        metadata.Properties().Select(p => p.Name).FirstOrDefault(name => !Fields.ContainsKey(name));

    /// <summary>
    /// The <c>set-metadata</c> options of <paramref name="metadata" />, whose keys are all known: text
    /// as is, numbers and booleans as invariant text, a list of keywords joined with ", " (Gotenberg 8
    /// wrote each as an item of the keyword list), dates as PDF dates. Null values are left out.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when a value cannot be written as its field, naming the key.</exception>
    internal static List<(string Name, object? Value)> ToStageOptions(JObject metadata)
    {
        var options = new List<(string Name, object? Value)>();
        var keyOfField = new Dictionary<string, string>();

        foreach (var property in metadata.Properties())
        {
            var field = Fields[property.Name];

            var value = field is StageOptions.CreationDate or StageOptions.ModDate
                ? ToPdfDate(property)
                : ToText(property, field == StageOptions.Keywords);

            if (value == null) continue;

            if (keyOfField.TryGetValue(field, out var other))
                throw new ArgumentException(
                    $"The metadata keys '{other}' and '{property.Name}' both set the {field} field on Gotenberg 9: keep one of them.",
                    nameof(PdfOutput.PdfOutputOptions.MetaData));

            keyOfField[field] = property.Name;
            options.Add((field, value));
        }

        return options;
    }

    private static string? ToText(JProperty property, bool isKeywords)
    {
        var token = property.Value;

        switch (token.Type)
        {
            case JTokenType.Null or JTokenType.Undefined:
                return null;

            case JTokenType.Array when isKeywords:
                if (token.Children().Any(item => item.Type != JTokenType.String))
                    throw Unwritable(property, "a list of keywords holds text only");

                return string.Join(", ", token.Children().Select(item => (string)item!));

            case JTokenType.Boolean:
                return (bool)token ? "true" : "false";

            case JTokenType.String or JTokenType.Integer or JTokenType.Float or JTokenType.Date or JTokenType.Guid or JTokenType.Uri:
                return ((JValue)token).ToString(CultureInfo.InvariantCulture);

            default:
                throw Unwritable(property, "Gotenberg 9 takes one text value for it");
        }
    }

    private static string? ToPdfDate(JProperty property)
    {
        var token = property.Value;

        switch (token.Type)
        {
            case JTokenType.Null or JTokenType.Undefined:
                return null;

            case JTokenType.Date when ((JValue)token).Value is DateTimeOffset offsetDate:
                return FormatPdfDate(offsetDate.DateTime, offsetDate.Offset);

            case JTokenType.Date:
                var date = (DateTime)token;

                return date.Kind switch
                {
                    DateTimeKind.Utc => FormatPdfDate(date, TimeSpan.Zero),
                    DateTimeKind.Local => FormatPdfDate(date, TimeZoneInfo.Local.GetUtcOffset(date)),
                    _ => FormatPdfDate(date, null)
                };

            case JTokenType.String:
                return ParseTextDate(property, ((string)token!).Trim());

            default:
                throw Unwritable(property, "a date is a date, or text such as 2006-09-18T16:27:50-04:00, 2006:09:18 16:27:50 or D:20060918162750-04'00'");
        }
    }

    private static string ParseTextDate(JProperty property, string text)
    {
        if (text.StartsWith("D:", StringComparison.Ordinal))
        {
            if (PdfDate.IsMatch(text)) return text;

            throw Unwritable(property, $"'{text}' is not a PDF date such as D:20060918162750-04'00'");
        }

        var match = TextDate.Match(text);

        if (!match.Success)
            throw Unwritable(property, $"'{text}' is not a date such as 2006-09-18T16:27:50-04:00, 2006:09:18 16:27:50 or D:20060918162750-04'00'");

        int Part(string name) =>
            match.Groups[name].Success ? int.Parse(match.Groups[name].Value, CultureInfo.InvariantCulture) : 0;

        var hasTime = match.Groups["h"].Success;

        try
        {
            var local = new DateTime(Part("y"), Part("mo"), Part("d"), Part("h"), Part("mi"), Part("s"));

            return FormatPdfDate(local, ParseOffset(match.Groups["z"]), hasTime);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw Unwritable(property, $"'{text}' is not a valid date");
        }
    }

    private static TimeSpan? ParseOffset(Group zone)
    {
        if (!zone.Success) return null;
        if (zone.Value == "Z") return TimeSpan.Zero;

        var digits = zone.Value.Substring(1).Replace(":", "");
        var hours = int.Parse(digits.Substring(0, 2), CultureInfo.InvariantCulture);
        var minutes = digits.Length > 2 ? int.Parse(digits.Substring(2, 2), CultureInfo.InvariantCulture) : 0;

        if (hours > 23 || minutes > 59) throw new ArgumentOutOfRangeException(nameof(zone));

        var offset = new TimeSpan(hours, minutes, 0);

        return zone.Value[0] == '-' ? offset.Negate() : offset;
    }

    /// <summary>
    /// <paramref name="date" /> as a PDF date, in its own offset.
    /// </summary>
    internal static string ToPdfDate(DateTimeOffset date) => FormatPdfDate(date.DateTime, date.Offset);

    /// <summary>
    /// D:YYYYMMDDHHmmSS and the offset: Z for UTC, +HH'mm' or -HH'mm' otherwise, nothing when unknown.
    /// </summary>
    private static string FormatPdfDate(DateTime local, TimeSpan? offset, bool hasTime = true)
    {
        var text = new StringBuilder("D:");

        text.Append(local.ToString(hasTime ? "yyyyMMddHHmmss" : "yyyyMMdd", CultureInfo.InvariantCulture));

        if (offset is { } value)
        {
            if (value == TimeSpan.Zero)
                text.Append('Z');
            else
                text.Append(value < TimeSpan.Zero ? '-' : '+')
                    .Append(Math.Abs(value.Hours).ToString("00", CultureInfo.InvariantCulture))
                    .Append('\'')
                    .Append(Math.Abs(value.Minutes).ToString("00", CultureInfo.InvariantCulture))
                    .Append('\'');
        }

        return text.ToString();
    }

    private static ArgumentException Unwritable(JProperty property, string reason) =>
        new(
            $"The metadata value of '{property.Name}' cannot be sent to Gotenberg 9: {reason}.",
            nameof(PdfOutput.PdfOutputOptions.MetaData));
}
