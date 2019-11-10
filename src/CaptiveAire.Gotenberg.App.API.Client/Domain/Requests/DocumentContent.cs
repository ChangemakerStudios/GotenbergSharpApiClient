// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// Represents the elements of a document
    /// </summary>
    /// <remarks>The file names are a Gotenberg Api convention</remarks>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DocumentContent
    {
        static readonly Type attribType = typeof(MultiFormHeaderAttribute);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentContent"/> class.
        /// </summary>
        /// <param name="bodyHtml">The body HTML.</param>
        /// <param name="headerHtml">The header HTML.</param>
        /// <param name="footerHtml">The footer HTML.</param>
        /// <exception cref="ArgumentOutOfRangeException">bodyHtml</exception>
        public DocumentContent(string bodyHtml, string footerHtml, string headerHtml = "")
        {
            if(bodyHtml.IsNotSet()) throw new ArgumentOutOfRangeException(nameof(bodyHtml));

            BodyHtml = bodyHtml;
            HeaderHtml = headerHtml;
            FooterHtml = footerHtml;
        }

        /// <summary>
        /// Gets the header HTML.
        /// </summary>
        /// <value>
        /// The header HTML.
        /// </value>
        [MultiFormHeader(fileName: "header.html")]
        public string HeaderHtml { get; }

        /// <summary>
        /// Gets the content HTML. This is the body of the document
        /// </summary>
        /// <value>
        /// The content HTML.
        /// </value>
        [MultiFormHeader(fileName: "index.html")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string BodyHtml { get; }

        /// <summary>
        /// Gets the footer HTML.
        /// </summary>
        /// <value>
        /// The footer HTML.
        /// </value>
        [MultiFormHeader(fileName: "footer.html")]
        public string FooterHtml { get; }

        /// <summary>
        /// Transforms the instance to a list of StringContent items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<StringContent> ToStringContent()
        {   
            return this.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, attribType))
                .Select(p=> new { Prop = p , Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, attribType) } )
                .Select(_ =>
                {
                    var value = _.Prop.GetValue(this);
                    var contentItem = new StringContent(value.ToString());

                    contentItem.Headers.ContentDisposition = 
                        new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) { Name = _.Attrib.Name, FileName = _.Attrib.FileName };

                    contentItem.Headers.ContentType = new MediaTypeHeaderValue(_.Attrib.MediaType);
                    
                    return contentItem;
                });
        }
        
    }
}