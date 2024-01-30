//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
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

#if NETSTANDARD2_0
// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

internal static class KeyValuePair
{
    /// <summary>
    ///     b/c Kvp.Create is not supported by netstandard2.0
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

#endif