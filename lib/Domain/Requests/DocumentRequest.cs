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
    [UsedImplicitly]
    public class DocumentRequest<TValue> : IConvertToHttpContent where TValue : class
    {
        readonly Func<TValue, HttpContent> _converter;
        readonly Type _attributeType = typeof(MultiFormHeaderAttribute);

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentRequest{TValue}"/>
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="bodyHtml"></param>
        internal DocumentRequest(Func<TValue, HttpContent> converter, TValue bodyHtml)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            BodyHtml = bodyHtml ?? throw new ArgumentNullException(nameof(bodyHtml));
        }

        /// <summary>
        /// Gets the header HTML.
        /// </summary>
        /// <value>
        /// The header HTML.
        /// </value>
        [UsedImplicitly]
        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Header)]
        public TValue HeaderHtml { get; set; }

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
        [UsedImplicitly]
        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Footer)]
        public TValue FooterHtml { get; set; }

        /// <summary>
        /// Transforms the instance to a list of StringContent items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, _attributeType))
                .Select(p => new { Prop = p, Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, _attributeType) })
                .Select(_ =>
                {
                    var value = _.Prop.GetValue(this);

                    if (value == null) return null;

                    var item = _converter((TValue)value);

                    item.Headers.ContentType = new MediaTypeHeaderValue(_.Attrib.MediaType);
                    item.Headers.ContentDisposition = new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) { Name = _.Attrib.Name, FileName = _.Attrib.FileName };

                    return item;

                }).Where(item => item != null);
        }

    }
}