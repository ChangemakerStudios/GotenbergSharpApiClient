using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>([CanBeNull] this IEnumerable<T> items)
            where T : class
        {
            return items.IfNullEmpty().Where(item => item != null);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<T> IfNullEmpty<T>([CanBeNull] this IEnumerable<T> items)
        {
            return items ?? Enumerable.Empty<T>();
        }
    }
}