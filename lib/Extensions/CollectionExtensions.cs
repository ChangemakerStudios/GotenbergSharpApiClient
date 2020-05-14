// Gotenberg.Sharp.Api.Client - Copyright (c) 2020 CaptiveAire

using System.Collections.Generic;
using System.Linq;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> IfNullEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        public static void AddRange<TKey, TValue>(
            this ICollection<KeyValuePair<TKey, TValue>> collection,
            IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var item in items.IfNullEmpty().ToList())
            {
                collection.Add(item);
            }
        }
    }
}