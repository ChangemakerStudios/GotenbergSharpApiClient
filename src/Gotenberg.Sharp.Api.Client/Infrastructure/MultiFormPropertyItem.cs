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

using System.Reflection;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

internal class MultiFormPropertyItem(PropertyInfo property, MultiFormHeaderAttribute attribute)
{
    private static readonly Type _attributeType = typeof(MultiFormHeaderAttribute);

    public PropertyInfo Property { get; } =
        property ?? throw new ArgumentNullException(nameof(property));

    public MultiFormHeaderAttribute Attribute { get; } =
        attribute ?? throw new ArgumentNullException(nameof(attribute));

    internal static IEnumerable<MultiFormPropertyItem> FromType(Type instanceType)
    {
        if (instanceType == null) throw new ArgumentNullException(nameof(instanceType));

        var propertyInfos = instanceType.GetProperties()
            .Where(prop => System.Attribute.IsDefined(prop, _attributeType)).ToList();

        foreach (var propertyInfo in propertyInfos)
            if (System.Attribute.GetCustomAttribute(
                    propertyInfo,
                    _attributeType) is MultiFormHeaderAttribute multiFormHeaderAttribute)
                yield return new MultiFormPropertyItem(
                    propertyInfo,
                    multiFormHeaderAttribute);
    }
}