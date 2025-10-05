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

namespace Gotenberg.Sharp.API.Client.Extensions;

public static class IntExtensions
{
    private const int AlphabetLength = 26;

    private static readonly char[] Alphabet = Enumerable.Range('A', 'Z' - 'A' + 1)
        .Select(c => (char)c)
        .ToArray();

    /// <summary>
    ///     Returns a-z for the first 26 (0-25); then za - zz for the next;
    ///     zza - zzz, etc. with the specified extension appended to the end
    /// </summary>
    /// <remarks>
    ///     https://gotenberg.dev/docs/modules/pdf-engines#merge
    /// </remarks>
    /// <param name="sortNumber"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static string ToAlphabeticallySortableFileName(this int sortNumber, string extension)
    {
        if (sortNumber < 0) throw new ArgumentOutOfRangeException(nameof(sortNumber));
        if (extension.IsNotSet())
            throw new ArgumentException("extension is either null or empty");

        return
            $"{new string('Z', sortNumber / AlphabetLength)}{Alphabet[sortNumber % AlphabetLength]}{extension}";
    }
}