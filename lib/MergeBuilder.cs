// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class MergeBuilder
    {
        IMergeRequest _request;

        [UsedImplicitly]
        public MergeBuilder(Dictionary<string, Stream> items) => 
                CreateAndSetRequest(value => new StreamContent(value), items);

        [UsedImplicitly]
        public MergeBuilder(Dictionary<string, byte[]> items) => 
                CreateAndSetRequest(value => new ByteArrayContent(value), items);

        [UsedImplicitly]
        public MergeBuilder(Dictionary<string,string> items)
            => CreateAndSetRequest(value => new StringContent(value), items);
 
        [UsedImplicitly]
        public MergeBuilder(IEnumerable<KeyValuePair<string, string>> items) 
                : this(new Dictionary<string, string>( items?.ToDictionary(_=> _.Key, _=> _.Value ) ?? throw new InvalidOperationException() ))
        {
        }

        [UsedImplicitly]
        public MergeBuilder(IEnumerable<KeyValuePair<string, Stream>> items) 
                : this(new Dictionary<string, Stream>( items?.ToDictionary(_=> _.Key, _=> _.Value ) ?? throw new InvalidOperationException() ))
        {
        }

         [UsedImplicitly]
        public MergeBuilder(IEnumerable<KeyValuePair<string, byte[]>> items) 
                : this(new Dictionary<string, byte[]>( items?.ToDictionary(_=> _.Key, _=> _.Value ) ?? throw new InvalidOperationException() ))
        {
        }


        /// <summary>
        ///  Configures individual requests, overriding container level settings that define defaults
        ///  In most cases the defaults are fine and there's no need to provide a custom configuration.   
        /// </summary>
        /// <param name="customConfig"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public MergeBuilder ConfigureWith(RequestConfig customConfig)
        {
            this._request.Config = customConfig;

            return this;
        }

        /// <summary>
        /// Builds the merge request
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        public IMergeRequest Build() => this._request;

        protected virtual void CreateAndSetRequest<TAsset>(Func<TAsset,HttpContent> converter, Dictionary<string, TAsset> items) where TAsset: class
        {
            this._request = new MergeRequest<TAsset>(converter) { Items = items ?? new Dictionary<string, TAsset>() };
        }
    }

}