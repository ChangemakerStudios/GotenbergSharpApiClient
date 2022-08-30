using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> WhereNotNull<T>([CanBeNull] this IEnumerable<T> items)
            where T : class
        {
            return items.IfNullEmpty().Where(item => item != null);
        }

        internal static IEnumerable<T> IfNullEmpty<T>([CanBeNull] this IEnumerable<T> items)
        {
            return items ?? Enumerable.Empty<T>();
        }
    }
}