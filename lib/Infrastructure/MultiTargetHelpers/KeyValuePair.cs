#if NETSTANDARD2_1
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Collections.Generic.KeyValuePair))]
#else
// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
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
#endif