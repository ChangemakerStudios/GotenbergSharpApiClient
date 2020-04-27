// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Content;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    [PublicAPI]
    public class MergeBuilder: BaseBuilder<MergeRequest> 
    {
        public MergeBuilder()
        {
        }

        protected override MergeRequest Request { get; set; }
      
        [PublicAPI]
        public MergeBuilder(Dictionary<string, ContentItem> items) => 
                this.Request.Items = items ?? new Dictionary<string, ContentItem>();

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
                : this(new Dictionary<string, ContentItem>( items?.ToDictionary(i=> i.Key, i=> i.Value ) ?? throw new InvalidOperationException() ))
        {
        }
        [PublicAPI]
        public ConfigBuilder<MergeBuilder> ConfigureRequest => new ConfigBuilder<MergeBuilder>(this.Request, this);
 
        /// <summary>
        /// Builds the merge request
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public IMergeRequest Build() => this.Request;
     
    }

}