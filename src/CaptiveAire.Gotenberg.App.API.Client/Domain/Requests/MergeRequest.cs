// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System.Collections.Generic;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// A request to merge the specified items into one pdf file
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MergeRequest
    {
        /// <summary>
        /// Key = file name; value = the pdf bytes
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public Dictionary<string, byte[]> Items { get; set; } = new Dictionary<string, byte[]>();
    }
}