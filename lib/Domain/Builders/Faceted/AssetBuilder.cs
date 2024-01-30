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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    public sealed class AssetBuilder
    {
        private readonly AssetDictionary _assets;

        internal AssetBuilder(AssetDictionary assets)
        {
            this._assets = assets;
        }

        [PublicAPI]
        public AssetBuilder AddItem(string name, ContentItem value)
        {
            // ReSharper disable once ComplexConditionExpression
            if (name.IsNotSet() || new FileInfo(name).Extension.IsNotSet()
                                || name.LastIndexOf('/') >= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(name),
                    "Asset names must be relative file names with extensions");
            }

            this._assets.Add(name, value ?? throw new ArgumentNullException(nameof(value)));

            return this;
        }

        [PublicAPI]
        public AssetBuilder AddItem(string name, string value) =>
            AddItem(name, new ContentItem(value));

        [PublicAPI]
        public AssetBuilder AddItem(string name, byte[] value) =>
            AddItem(name, new ContentItem(value));

        [PublicAPI]
        public AssetBuilder AddItem(string name, Stream value) =>
            AddItem(name, new ContentItem(value));

        #region 'n' assets

        #region from dictionaries

        [PublicAPI]
        public AssetBuilder AddItems(Dictionary<string, ContentItem>? items)
        {
            foreach (var item in items.IfNullEmpty())
            {
                this.AddItem(item.Key, item.Value);
            }

            return this;
        }

        [PublicAPI]
        public AssetBuilder AddItems(Dictionary<string, string>? assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)));

        [PublicAPI]
        public AssetBuilder AddItems(Dictionary<string, byte[]>? assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)));

        [PublicAPI]
        public AssetBuilder AddItems(Dictionary<string, Stream>? assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)));

        #endregion

        #region from KVP enumerables

        [PublicAPI]
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, ContentItem>> assets) =>
            AddItems(
                new Dictionary<string, ContentItem>(
                    assets?.ToDictionary(a => a.Key, a => a.Value) ??
                    throw new ArgumentNullException(nameof(assets))));

        [PublicAPI]
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, string>> assets) =>
            AddItems(
                new Dictionary<string, ContentItem>(
                    assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)) ??
                    throw new ArgumentNullException(nameof(assets))));

        [PublicAPI]
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, byte[]>> assets) =>
            AddItems(
                assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)) ??
                throw new ArgumentNullException(nameof(assets)));

        [PublicAPI]
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, Stream>> assets) =>
            AddItems(
                assets?.ToDictionary(s => s.Key, a => new ContentItem(a.Value)) ??
                throw new ArgumentNullException(nameof(assets)));

        #endregion

        #endregion
    }
}