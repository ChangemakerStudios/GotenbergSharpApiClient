using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Gotenberg.Sharp.API.Client.Infrastructure.MultiTargetHelpers
{
    internal static class KeyValuePair
    {
        /// <summary>
        ///  b/c Kvp.Create is not supported by netstandard2.0
        /// </summary>
        /// <typeparam name="TKey">   Type of the key.</typeparam>
        /// <typeparam name="TValue"> Type of the value.</typeparam>
        /// <param name="key">   The key.</param>
        /// <param name="value"> The value.</param>
        /// <returns>
        /// </returns>
        internal static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}
