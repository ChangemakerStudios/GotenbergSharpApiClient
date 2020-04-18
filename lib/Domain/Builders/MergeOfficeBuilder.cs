// Gotenberg.Sharp.Api.Client - Copyright (c) 2020 CaptiveAire

using System.Collections.Generic;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Content;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public class MergeOfficeBuilder : MergeBuilder
    {
        readonly IMergeOfficeRequest _request;

        [PublicAPI]
        public MergeOfficeBuilder(Dictionary<string, ContentItem> items) =>
                this._request = new MergeOfficeRequest( items ?? new Dictionary<string, ContentItem>() );

        [PublicAPI]
        public new IMergeOfficeRequest Build() => this._request;
    }
}