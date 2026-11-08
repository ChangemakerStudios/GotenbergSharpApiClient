//  Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

namespace Gotenberg.Sharp.API.Client.Infrastructure.Json;

/// <summary>
/// Writes loosely typed values (metadata, extra headers, watermark/stamp options) as JSON by
/// inspecting each value's runtime type, so no reflection-based serializer is needed.
/// </summary>
internal static class JsonObjectWriter
{
    private static readonly JsonWriterOptions _writerOptions = new() { Encoder = GotenbergJsonContext.Encoder };

    internal static string ToJson(IEnumerable<KeyValuePair<string, object>> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream, _writerOptions))
        {
            WriteObject(writer, values!);
        }

        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
    }

    /// <summary>
    /// Copies the caller's dictionary so later changes to it do not leak into the request, and writes it
    /// once so an unsupported value fails where it is supplied rather than when the request is sent.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when a value cannot be written as JSON.</exception>
    internal static IDictionary<string, object> Snapshot(IDictionary<string, object> values)
    {
        var copy = new Dictionary<string, object>(values);

        ToJson(copy);

        return copy;
    }

    /// <summary>
    /// Copies a <see cref="JsonObject" /> into the dictionary shape the request types store.
    /// </summary>
    internal static IDictionary<string, object> ToDictionary(JsonObject jsonObject)
    {
        if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));

        var result = new Dictionary<string, object>();

        // A JSON null stays a null entry; it is written back out as null.
        foreach (var property in jsonObject)
            result[property.Key] = property.Value!;

        return result;
    }

    internal static void WriteValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string stringValue:
                writer.WriteStringValue(stringValue);
                break;
            case bool boolValue:
                writer.WriteBooleanValue(boolValue);
                break;
            case int intValue:
                writer.WriteNumberValue(intValue);
                break;
            case long longValue:
                writer.WriteNumberValue(longValue);
                break;
            case double doubleValue:
                writer.WriteNumberValue(doubleValue);
                break;
            case decimal decimalValue:
                writer.WriteNumberValue(decimalValue);
                break;
            case float floatValue:
                writer.WriteNumberValue(floatValue);
                break;
            case uint uintValue:
                writer.WriteNumberValue(uintValue);
                break;
            case ulong ulongValue:
                writer.WriteNumberValue(ulongValue);
                break;
            case short shortValue:
                writer.WriteNumberValue(shortValue);
                break;
            case ushort ushortValue:
                writer.WriteNumberValue(ushortValue);
                break;
            case byte byteValue:
                writer.WriteNumberValue(byteValue);
                break;
            case sbyte sbyteValue:
                writer.WriteNumberValue(sbyteValue);
                break;
            case char charValue:
                writer.WriteStringValue(charValue.ToString());
                break;
            case DateTime dateTimeValue:
                writer.WriteStringValue(dateTimeValue);
                break;
            case DateTimeOffset dateTimeOffsetValue:
                writer.WriteStringValue(dateTimeOffsetValue);
                break;
            case Guid guidValue:
                writer.WriteStringValue(guidValue);
                break;
            case Uri uriValue:
                writer.WriteStringValue(uriValue.ToString());
                break;
            case byte[] bytesValue:
                writer.WriteBase64StringValue(bytesValue);
                break;
            case Enum enumValue:
                writer.WriteNumberValue(Convert.ToInt64(enumValue, CultureInfo.InvariantCulture));
                break;
            case JsonNode nodeValue:
                nodeValue.WriteTo(writer);
                break;
            case JsonElement elementValue:
                elementValue.WriteTo(writer);
                break;
            case JsonDocument documentValue:
                documentValue.RootElement.WriteTo(writer);
                break;
            case IEnumerable<KeyValuePair<string, object?>> objectValue:
                WriteObject(writer, objectValue);
                break;
            case IDictionary dictionaryValue:
                writer.WriteStartObject();
                foreach (DictionaryEntry entry in dictionaryValue)
                {
                    writer.WritePropertyName(Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? string.Empty);
                    WriteValue(writer, entry.Value);
                }

                writer.WriteEndObject();
                break;
            case IEnumerable enumerableValue:
                writer.WriteStartArray();
                foreach (var item in enumerableValue)
                    WriteValue(writer, item);
                writer.WriteEndArray();
                break;
            default:
                throw new NotSupportedException(
                    $"Values of type '{value.GetType()}' cannot be written as JSON. Use strings, numbers, booleans, "
                    + "dates, dictionaries, collections, or a System.Text.Json JsonNode/JsonElement.");
        }
    }

    private static void WriteObject(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, object?>> values)
    {
        writer.WriteStartObject();

        foreach (var entry in values)
        {
            writer.WritePropertyName(entry.Key);
            WriteValue(writer, entry.Value);
        }

        writer.WriteEndObject();
    }
}
