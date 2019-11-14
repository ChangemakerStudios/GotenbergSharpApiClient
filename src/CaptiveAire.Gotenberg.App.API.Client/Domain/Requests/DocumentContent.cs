// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure;
using JetBrains.Annotations;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// Represents the elements of a document
    /// </summary>
    /// <remarks>The file names are a Gotenberg Api convention</remarks>
     public class DocumentContent<TValue>
    {
        readonly Type _attributeType = typeof(MultiFormHeaderAttribute);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentContent{TValue}"/>
        /// </summary>
        /// <param name="bodyHtml">The body HTML.</param>
        /// <param name="headerHtml">The header HTML.</param>
        /// <param name="footerHtml">The footer HTML.</param>
        /// <exception cref="ArgumentOutOfRangeException">bodyHtml</exception>
        public DocumentContent(TValue bodyHtml, TValue footerHtml, TValue headerHtml)
        {
            if(bodyHtml.Equals(default(TValue))) throw new ArgumentOutOfRangeException(nameof(bodyHtml));

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
        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Header)]
        public TValue HeaderHtml { get; }

        /// <summary>
        /// Gets the content HTML. This is the body of the document
        /// </summary>
        /// <value>
        /// The content HTML.
        /// </value>
        [UsedImplicitly]
        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Index)] 
        public TValue BodyHtml { get; }

        /// <summary>
        /// Gets the footer HTML.
        /// </summary>
        /// <value>
        /// The footer HTML.
        /// </value>
        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Footer)]
        public TValue FooterHtml { get; }

        /// <summary>
        /// Transforms the instance to a list of StringContent items
        /// </summary>
        /// <returns></returns>

        //    internal static IEnumerable<HttpContent> ToHttpContent<TValue>(this Dictionary<string, TValue> assets, Func<TValue,HttpContent> converter)

        internal IEnumerable<HttpContent> ToHttpContent(Func<TValue,HttpContent> converter)
        {   
            return this.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, _attributeType))
                .Select(p=> new { Prop = p, Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, _attributeType) })
                .Select(_ =>
                {
                    var value = (TValue)_.Prop.GetValue(this);
                    var contentItem = converter(value);

                    contentItem.Headers.ContentDisposition = 
                        new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) { Name = _.Attrib.Name, FileName = _.Attrib.FileName };

                    contentItem.Headers.ContentType = new MediaTypeHeaderValue(_.Attrib.MediaType);
                    
                    return contentItem;
                });
        }
        
    }
}