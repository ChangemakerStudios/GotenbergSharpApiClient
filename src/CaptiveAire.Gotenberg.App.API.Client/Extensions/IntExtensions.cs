// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Linq;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static class IntExtensions
    {
        const int alphabetLength = 26;
        static readonly char[] _alphabet = Enumerable.Range('A', 'Z' - 'A' + 1).Select(c => (char) c).ToArray();

        /// <summary>
        /// Returns a-z for the first 26 (0-25); then za - zz for the next; zza - zzz, etc.
        /// </summary>
        /// <remarks>
        ///     Useful for merging pdfs. Gotenberg merges them alphabetically by file name
        ///     https://thecodingmachine.github.io/gotenberg/#merge
        /// </remarks>
        /// <param name="sortNumber"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public static string ToAlphabeticallySortableName(this int sortNumber)
        {
            if(sortNumber < 0) throw new ArgumentOutOfRangeException(nameof(sortNumber));

            return $"{new string('Z', sortNumber / alphabetLength)}{_alphabet[sortNumber % alphabetLength]}";
        }
    }
}