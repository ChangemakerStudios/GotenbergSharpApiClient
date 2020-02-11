// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    [PublicAPI]
    public class MergeBuilder
    {
        readonly IMergeRequest _request;

        [PublicAPI]
        public MergeBuilder(Dictionary<string, ContentItem> items) => 
                this._request = new MergeRequest { Items = items ?? new Dictionary<string, ContentItem>() };

        [PublicAPI]
        public MergeBuilder(Dictionary<string, string> items) 
                : this(items.ToDictionary(item => item.Key, item => new ContentItem(item.Value)))
        {
        }
        
        [PublicAPI]
        public MergeBuilder(Dictionary<string, byte[]> items) 
                : this(items.ToDictionary(item => item.Key, item => new ContentItem(item.Value)))
        {
        }
                
        [PublicAPI]
        public MergeBuilder(Dictionary<string, Stream> items) 
                : this(items.ToDictionary(item => item.Key, item => new ContentItem(item.Value)))
        {
        }
        
        [PublicAPI]
        public MergeBuilder(IEnumerable<KeyValuePair<string, ContentItem>> items) 
                : this(new Dictionary<string, ContentItem>( items?.ToDictionary(_=> _.Key, _=> _.Value ) ?? throw new InvalidOperationException() ))
        {
        }

        /// <summary>
        ///  Configures individual requests, overriding container level settings that define defaults
        ///  In most cases the defaults are fine and there's no need to provide a custom configuration.   
        /// </summary>
        /// <param name="customConfig"></param>
        /// <returns></returns>
        [PublicAPI]
        public MergeBuilder ConfigureWith(RequestConfig customConfig)
        {
            this._request.Config = customConfig;

            return this;
        }

        /// <summary>
        /// Builds the merge request
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public IMergeRequest Build() => this._request;
     
    }

}