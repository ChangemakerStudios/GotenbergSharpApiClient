// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class GotenbergSharpRequestExtensions
    {
        /// <summary>
        /// Transforms the specified request into a collection of <see cref="HttpContent" /> items
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static IEnumerable<StringContent> ToHttpContentCollection(this GotenbergSharpRequest request)
        {
            var docParts = ToHttpContentCollection(request, request.Content.GetType());
            var dimParts = ToHttpContentCollection(request, request.Dimensions.GetType());
            docParts.AddRange(dimParts);

            return docParts;
        }

        static List<StringContent> ToHttpContentCollection(GotenbergSharpRequest request, Type type)
        {
            return type.GetProperties()
                       .Where(prop => Attribute.IsDefined(prop, typeof(MultiFormHeaderAttribute)))
                       .Select(p=> new { Prop = p , Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, typeof(MultiFormHeaderAttribute)) } )
                       .Select(_ =>
                               {
                                   var isForDimensions = type == typeof(DocumentDimensions);
                                   var fileName = isForDimensions ? null : _.Attrib.FileName;

                                   var value = _.Prop.GetValue( isForDimensions ? request.Dimensions : (object)request.Content);
                                   var contentItem = new StringContent(value.ToString());
                                   contentItem.Headers.ContentDisposition = new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) {Name = _.Attrib.Name, FileName = fileName};

                                   if (!isForDimensions)
                                   {
                                       contentItem.Headers.ContentType = new MediaTypeHeaderValue(_.Attrib.MediaType);
                                   }

                                   return contentItem;
                               }).ToList();
        }

    }
}