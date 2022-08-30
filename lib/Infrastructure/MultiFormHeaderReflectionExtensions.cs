using System;
using System.Collections.Generic;
using System.Linq;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

internal static class MultiFormHeaderReflectionExtensions
{
    static readonly Type AttributeType = typeof(MultiFormHeaderAttribute);

    internal static IEnumerable<MultiFormPropertyItem> ToMultiFormPropertyItems(this Type instanceType)
    {
        return instanceType.GetProperties()
            .Where(prop => Attribute.IsDefined(prop, AttributeType))
            .Select(p =>
                new MultiFormPropertyItem {
                    Property = p,
                    Attribute = (MultiFormHeaderAttribute) Attribute.GetCustomAttribute(p, AttributeType)
            });
    }
}