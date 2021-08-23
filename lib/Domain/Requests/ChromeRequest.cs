using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Infrastructure;

using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class ChromeRequest : RequestBase
    {
        
        JObject _extraHeaders;

        public Dimensions Dimensions { get; set; } = Dimensions.ToChromeDefaults();
 
        /// <summary>
        ///  Adds HTTP headers to send by Chromium while loading the HTML document
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        public void AddExtraHeaders(string headerName, string headerValue)
        {
            _extraHeaders = new JObject(new JProperty(headerName, headerValue));
        }

        protected IEnumerable<HttpContent> GetExtraHeaderHttpContent()
        {
            if (_extraHeaders != null)
            {
                yield return CreateFormDataItem(_extraHeaders, Constants.Gotenberg.CustomRemoteHeaders.ExtraHttpHeaders);
            }
        }
    }
}