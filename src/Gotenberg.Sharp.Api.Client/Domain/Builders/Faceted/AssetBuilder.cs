// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    /// <summary>
    /// Adds embedded resources (assets) to PDF conversion requests. Assets are files referenced by your HTML content
    /// such as images, fonts, CSS stylesheets, and JavaScript files. The asset name must match exactly how it's
    /// referenced in your HTML (e.g., "logo.png" for &lt;img src="logo.png"/&gt;).
    /// </summary>
    public sealed class AssetBuilder
    {
        private readonly AssetDictionary _assets;

        internal AssetBuilder(AssetDictionary assets)
        {
            this._assets = assets;
        }

        /// <summary>
        /// Adds a single asset file. The name must be a relative filename with extension, matching the reference in your HTML.
        /// </summary>
        /// <param name="name">Filename as referenced in HTML (e.g., "styles.css", "logo.png"). Must include extension and not contain path separators.</param>
        /// <param name="value">The file content.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when name is invalid (missing extension or contains path separators).</exception>
        public AssetBuilder AddItem(string name, ContentItem value)
        {
            // ReSharper disable once ComplexConditionExpression
            if (name.IsNotSet() || new FileInfo(name).Extension.IsNotSet() || name.LastIndexOf('/') >= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(name),
                    "Asset names must be relative file names with extensions");
            }

            this._assets.Add(name, value ?? throw new ArgumentNullException(nameof(value)));

            return this;
        }

        /// <summary>
        /// Adds a single asset from string content (typically for text files like CSS or JavaScript).
        /// </summary>
        /// <param name="name">Filename as referenced in HTML.</param>
        /// <param name="value">File content as string.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItem(string name, string value) => AddItem(name, new ContentItem(value));

        /// <summary>
        /// Adds a single asset from byte array (typically for binary files like images or fonts).
        /// </summary>
        /// <param name="name">Filename as referenced in HTML.</param>
        /// <param name="value">File content as bytes.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItem(string name, byte[] value) => AddItem(name, new ContentItem(value));

        /// <summary>
        /// Adds a single asset from a stream.
        /// </summary>
        /// <param name="name">Filename as referenced in HTML.</param>
        /// <param name="value">Stream containing file content.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItem(string name, Stream value) => AddItem(name, new ContentItem(value));

        #region 'n' assets

        #region from dictionaries

        /// <summary>
        /// Adds multiple assets from a dictionary. Useful for adding several files at once.
        /// </summary>
        /// <param name="items">Dictionary of filename to content mappings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItems(Dictionary<string, ContentItem>? items)
        {
            foreach (var item in items.IfNullEmpty())
            {
                this.AddItem(item.Key, item.Value);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple assets from a dictionary with string content.
        /// </summary>
        /// <param name="assets">Dictionary of filename to string content mappings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItems(Dictionary<string, string>? assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)));

        /// <summary>
        /// Adds multiple assets from a dictionary with byte array content.
        /// </summary>
        /// <param name="assets">Dictionary of filename to byte array mappings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItems(Dictionary<string, byte[]>? assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)));

        /// <summary>
        /// Adds multiple assets from a dictionary with stream content.
        /// </summary>
        /// <param name="assets">Dictionary of filename to stream mappings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItems(Dictionary<string, Stream>? assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)));

        #endregion

        #region from KVP enumerables

        /// <summary>
        /// Adds multiple assets from a key-value pair enumerable.
        /// </summary>
        /// <param name="assets">Enumerable of filename to content mappings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, ContentItem>> assets) =>
            AddItems(
                new Dictionary<string, ContentItem>(
                    assets?.ToDictionary(a => a.Key, a => a.Value) ?? throw new ArgumentNullException(nameof(assets))));

        /// <summary>
        /// Adds multiple assets from a key-value pair enumerable with string content.
        /// </summary>
        /// <param name="assets">Enumerable of filename to string content mappings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, string>> assets) =>
            AddItems(
                new Dictionary<string, ContentItem>(
                    assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value))
                    ?? throw new ArgumentNullException(nameof(assets))));

        /// <summary>
        /// Adds multiple assets from a key-value pair enumerable with byte array content.
        /// </summary>
        /// <param name="assets">Enumerable of filename to byte array mappings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, byte[]>> assets) =>
            AddItems(
                assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value))
                ?? throw new ArgumentNullException(nameof(assets)));

        /// <summary>
        /// Adds multiple assets from a key-value pair enumerable with stream content.
        /// </summary>
        /// <param name="assets">Enumerable of filename to stream mappings.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, Stream>> assets) =>
            AddItems(
                assets?.ToDictionary(s => s.Key, a => new ContentItem(a.Value))
                ?? throw new ArgumentNullException(nameof(assets)));

        #endregion

        #endregion
    }
}