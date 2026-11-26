using System.Text.Json.Nodes;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// Parses the JSON of a form field or a response in the tests.
/// </summary>
internal static class TestJson
{
    internal static JsonArray ParseArray(string json) => JsonNode.Parse(json)!.AsArray();

    internal static JsonObject ParseObject(string json) => JsonNode.Parse(json)!.AsObject();
}
