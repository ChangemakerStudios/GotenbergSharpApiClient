//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

internal class MultiFormPropertyItem
{
    private static readonly Type _attributeType = typeof(MultiFormHeaderAttribute);

    public MultiFormPropertyItem(PropertyInfo property, MultiFormHeaderAttribute attribute)
    {
        this.Property = property ?? throw new ArgumentNullException(nameof(property));
        this.Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
    }

    public PropertyInfo Property { get; }

    public MultiFormHeaderAttribute Attribute { get; }

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