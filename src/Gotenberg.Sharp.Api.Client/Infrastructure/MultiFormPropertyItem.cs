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

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

internal class MultiFormPropertyItem(
    PropertyInfo property,
    MultiFormHeaderAttribute attribute,
    string? gotenberg9Name = null)
{
    private static readonly Type _attributeType = typeof(MultiFormHeaderAttribute);

    private static readonly ConcurrentDictionary<Type, MultiFormPropertyItem[]> _itemsByType = new();

    public PropertyInfo Property { get; } =
        property ?? throw new ArgumentNullException(nameof(property));

    public MultiFormHeaderAttribute Attribute { get; } =
        attribute ?? throw new ArgumentNullException(nameof(attribute));

    /// <summary>
    ///     The name of the form field on Gotenberg 9, or null when it is the same as on Gotenberg 7 and 8.
    /// </summary>
    public string? Gotenberg9Name { get; } = gotenberg9Name;

    /// <summary>
    ///     The form field name for <paramref name="api" />.
    /// </summary>
    public string NameFor(GotenbergApiVersion api) =>
        api == GotenbergApiVersion.V9 ? this.Gotenberg9Name ?? this.Attribute.Name : this.Attribute.Name;

    internal static IEnumerable<MultiFormPropertyItem> FromType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type instanceType)
    {
        if (instanceType == null) throw new ArgumentNullException(nameof(instanceType));

        if (_itemsByType.TryGetValue(instanceType, out var items)) return items;

        // Not GetOrAdd(instanceType, ReadType): a delegate would drop the trimming annotation.
        return _itemsByType.GetOrAdd(instanceType, ReadType(instanceType));
    }

    private static MultiFormPropertyItem[] ReadType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type instanceType)
    {
        var propertyInfos = instanceType.GetProperties()
            .Where(prop => System.Attribute.IsDefined(prop, _attributeType)).ToList();

        var items = new List<MultiFormPropertyItem>();

        foreach (var propertyInfo in propertyInfos)
            if (System.Attribute.GetCustomAttribute(
                    propertyInfo,
                    _attributeType) is MultiFormHeaderAttribute multiFormHeaderAttribute)
                items.Add(
                    new MultiFormPropertyItem(
                        propertyInfo,
                        multiFormHeaderAttribute,
                        (System.Attribute.GetCustomAttribute(
                            propertyInfo,
                            typeof(Gotenberg9FieldAttribute)) as Gotenberg9FieldAttribute)?.Name));

        return items.ToArray();
    }
}