using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Infrastructure;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
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
        /// <param name="bodyHtml"></param>
        public DocumentContent(TValue bodyHtml)
        {
            if (bodyHtml.Equals(default(TValue))) throw new ArgumentOutOfRangeException(nameof(bodyHtml));

            BodyHtml = bodyHtml;
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
        internal IEnumerable<HttpContent> ToHttpContent(Func<TValue, HttpContent> converter)
        {
            return this.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, _attributeType))
                .Select(p => new { Prop = p, Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, _attributeType) })
                .Select(_ =>
                {
                    var value = _.Prop.GetValue(this);

                    if (value == null) return null;

                    var contentItem = converter((TValue)value);
                    contentItem.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) { Name = _.Attrib.Name, FileName = _.Attrib.FileName };
                    contentItem.Headers.ContentType = new MediaTypeHeaderValue(_.Attrib.MediaType);

                    return contentItem;

                }).Where(item => item != null);
        }

    }
}