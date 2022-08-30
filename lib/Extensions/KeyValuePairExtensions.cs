using System.Collections.Generic;

namespace Gotenberg.Sharp.API.Client.Extensions;

internal static class KeyValuePairExtensions
{
    internal static bool IsValid<TValue>(this KeyValuePair<string, TValue> pair) where TValue : class
    {
        return pair.Key.IsSet() && pair.Value != default(TValue);
    }
}