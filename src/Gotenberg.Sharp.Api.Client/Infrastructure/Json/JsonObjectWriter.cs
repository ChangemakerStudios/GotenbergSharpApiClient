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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace Gotenberg.Sharp.API.Client.Infrastructure.Json;

/// <summary>
/// Writes loosely typed values (metadata, extra headers, watermark/stamp options) as JSON by
/// inspecting each value's runtime type, so no reflection-based serializer is needed.
/// </summary>
/// <remarks>
/// Supported values: <see langword="null" />, <see cref="string" />, <see cref="char" />, <see cref="bool" />,
/// <see cref="byte" />, <see cref="sbyte" />, <see cref="short" />, <see cref="ushort" />, <see cref="int" />,
/// <see cref="uint" />, <see cref="long" />, <see cref="ulong" />, <see cref="float" />, <see cref="double" />,
/// <see cref="decimal" />, <see cref="DateTime" />, <see cref="DateTimeOffset" />, <see cref="Guid" />,
/// <see cref="Uri" />, enums (written as their number), <see cref="T:byte[]" /> (written as base64),
/// <see cref="JsonNode" />, <see cref="JsonElement" />, <see cref="JsonDocument" />, dictionaries and other
/// collections of supported values. Anything else, a non-finite <see cref="float" /> or <see cref="double" />,
/// or a collection that contains itself throws <see cref="NotSupportedException" />.
/// </remarks>
internal static class JsonObjectWriter
{
    private static readonly JsonWriterOptions _writerOptions = new() { Encoder = GotenbergJsonContext.Encoder };

    /// <exception cref="NotSupportedException">Thrown when a value cannot be written as JSON.</exception>
    internal static string ToJson(IEnumerable<KeyValuePair<string, object>> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream, _writerOptions))
        {
            WriteObject(writer, values!, new ReferenceSet());
        }

        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
    }

    /// <summary>
    /// Deep-copies the caller's dictionary, including nested dictionaries, collections, byte arrays and JSON
    /// values, so later changes to any of them do not leak into the request. The copy is written once so an
    /// unsupported value fails where it is supplied rather than when the request is sent.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when a value cannot be written as JSON.</exception>
    internal static IDictionary<string, object> Snapshot(IDictionary<string, object> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        var visiting = new ReferenceSet();
        visiting.Enter(values);

        var copy = CopyObject(values!, visiting);

        ToJson(copy);

        return copy;
    }

    /// <summary>
    /// Copies a <see cref="JsonObject" /> into the dictionary shape the request types store.
    /// </summary>
    /// <param name="jsonObject">The object to copy.</param>
    /// <param name="paramName">The caller's parameter name, reported when <paramref name="jsonObject" /> is null.</param>
    internal static IDictionary<string, object> ToDictionary(JsonObject jsonObject, string paramName)
    {
        if (jsonObject == null) throw new ArgumentNullException(paramName);

        var result = new Dictionary<string, object>();

        // A JSON null stays a null entry; it is written back out as null. The nodes still belong to the
        // caller's object here; Snapshot clones them.
        foreach (var property in jsonObject)
            result[property.Key] = property.Value!;

        return result;
    }

    private static Dictionary<string, object> CopyObject(
        IEnumerable<KeyValuePair<string, object?>> values,
        ReferenceSet visiting)
    {
        var copy = new Dictionary<string, object>();

        foreach (var entry in values)
            copy[entry.Key] = Copy(entry.Value, visiting)!;

        return copy;
    }

    private static object? Copy(object? value, ReferenceSet visiting)
    {
        switch (value)
        {
            case null:
            case string:
                return value;
            case byte[] bytesValue:
                return bytesValue.Clone();
            case JsonNode nodeValue:
                return nodeValue.DeepClone();
            case JsonElement elementValue:
                return elementValue.Clone();
            case JsonDocument documentValue:
                return documentValue.RootElement.Clone();
            case IEnumerable<KeyValuePair<string, object?>> objectValue:
            {
                visiting.Enter(objectValue);
                var copy = CopyObject(objectValue, visiting);
                visiting.Exit(objectValue);
                return copy;
            }
            case IDictionary dictionaryValue:
            {
                visiting.Enter(dictionaryValue);
                var copy = new Dictionary<string, object>();
                foreach (DictionaryEntry entry in dictionaryValue)
                    copy[KeyToString(entry.Key)] = Copy(entry.Value, visiting)!;
                visiting.Exit(dictionaryValue);
                return copy;
            }
            case IEnumerable enumerableValue:
            {
                visiting.Enter(enumerableValue);
                var copy = new List<object?>();
                foreach (var item in enumerableValue)
                    copy.Add(Copy(item, visiting));
                visiting.Exit(enumerableValue);
                return copy;
            }
            default:
                // Scalars are immutable. Unsupported types are kept as they are and rejected when written.
                return value;
        }
    }

    private static void WriteValue(Utf8JsonWriter writer, object? value, ReferenceSet visiting)
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
                if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue)) throw NotFinite(doubleValue);
                writer.WriteNumberValue(doubleValue);
                break;
            case decimal decimalValue:
                writer.WriteNumberValue(decimalValue);
                break;
            case float floatValue:
                if (float.IsNaN(floatValue) || float.IsInfinity(floatValue)) throw NotFinite(floatValue);
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
                WriteObject(writer, objectValue, visiting);
                break;
            case IDictionary dictionaryValue:
                visiting.Enter(dictionaryValue);
                writer.WriteStartObject();
                foreach (DictionaryEntry entry in dictionaryValue)
                {
                    writer.WritePropertyName(KeyToString(entry.Key));
                    WriteValue(writer, entry.Value, visiting);
                }

                writer.WriteEndObject();
                visiting.Exit(dictionaryValue);
                break;
            case IEnumerable enumerableValue:
                visiting.Enter(enumerableValue);
                writer.WriteStartArray();
                foreach (var item in enumerableValue)
                    WriteValue(writer, item, visiting);
                writer.WriteEndArray();
                visiting.Exit(enumerableValue);
                break;
            default:
                throw new NotSupportedException(
                    $"Values of type '{value.GetType()}' cannot be written as JSON. Use strings, chars, booleans, "
                    + "numbers, DateTime, DateTimeOffset, Guid, Uri, enums, byte arrays, dictionaries, collections, "
                    + "or a System.Text.Json JsonNode/JsonElement/JsonDocument.");
        }
    }

    private static void WriteObject(
        Utf8JsonWriter writer,
        IEnumerable<KeyValuePair<string, object?>> values,
        ReferenceSet visiting)
    {
        visiting.Enter(values);
        writer.WriteStartObject();

        foreach (var entry in values)
        {
            writer.WritePropertyName(entry.Key);
            WriteValue(writer, entry.Value, visiting);
        }

        writer.WriteEndObject();
        visiting.Exit(values);
    }

    private static string KeyToString(object key)
    {
        return Convert.ToString(key, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static NotSupportedException NotFinite(object value)
    {
        return new NotSupportedException(
            $"The number {Convert.ToString(value, CultureInfo.InvariantCulture)} cannot be written as JSON. "
            + "NaN and infinity have no JSON representation.");
    }

    /// <summary>
    /// The collections currently being walked, compared by reference, so a collection that contains
    /// itself is reported instead of recursing until the writer's depth limit.
    /// </summary>
    private sealed class ReferenceSet
    {
        private readonly HashSet<object> _items = new(ReferenceComparer.Instance);

        internal void Enter(object value)
        {
            if (!_items.Add(value))
            {
                throw new NotSupportedException(
                    $"A value of type '{value.GetType()}' contains a reference to itself and cannot be written as JSON.");
            }
        }

        internal void Exit(object value)
        {
            _items.Remove(value);
        }
    }

    private sealed class ReferenceComparer : IEqualityComparer<object>
    {
        internal static readonly ReferenceComparer Instance = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
