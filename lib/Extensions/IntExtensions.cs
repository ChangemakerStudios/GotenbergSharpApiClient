using System;
using System.Linq;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class IntExtensions
    {
        const int AlphabetLength = 26;

        // ReSharper disable once ComplexConditionExpression
        static readonly char[] Alphabet = Enumerable.Range('A', 'Z' - 'A' + 1).Select(c => (char) c).ToArray();

        /// <summary>
        /// Returns a-z for the first 26 (0-25); then za - zz for the next; zza - zzz, etc. with the specified extension appended to the end
        /// </summary>
        /// <remarks>
        ///     https://thecodingmachine.github.io/gotenberg/#merge
        /// </remarks>
        /// <param name="sortNumber"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string ToAlphabeticallySortableFileName(this int sortNumber, string extension)
        {
            if (sortNumber < 0) throw new ArgumentOutOfRangeException(nameof(sortNumber));
            if (extension.IsNotSet()) throw new ArgumentException("extension is either null or empty");

            return $"{new string('Z', sortNumber / AlphabetLength)}{Alphabet[sortNumber % AlphabetLength]}{extension}";
        }

       
    }
}