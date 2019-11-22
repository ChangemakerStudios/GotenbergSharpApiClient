// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    [UsedImplicitly]
    public class MergeOfficeBuilder : MergeBuilder
    {
        IMergeOfficeRequest _request;
        
        public MergeOfficeBuilder(Dictionary<string, Stream> items) : base(items)
        {
        }

        public MergeOfficeBuilder(Dictionary<string, byte[]> items) : base(items)
        {
        }

        public MergeOfficeBuilder(Dictionary<string, string> items) : base(items)
        {
        }

        public MergeOfficeBuilder(IEnumerable<KeyValuePair<string, string>> items) : base(items)
        {
        }

        public MergeOfficeBuilder(IEnumerable<KeyValuePair<string, Stream>> items) : base(items)
        {
        }

        public MergeOfficeBuilder(IEnumerable<KeyValuePair<string, byte[]>> items) : base(items)
        {
        }

        [UsedImplicitly]
        public new IMergeOfficeRequest Build() => this._request;

        protected override void CreateAndSetRequest<TAsset>(Func<TAsset, HttpContent> converter, Dictionary<string, TAsset> items)
        {
            _request = new MergeOfficeRequest<TAsset>(converter, items);
        }
    }
}