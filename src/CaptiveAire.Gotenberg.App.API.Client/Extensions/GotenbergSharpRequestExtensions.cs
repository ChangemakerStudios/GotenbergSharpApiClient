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

            var docParts = request.Content
                                  .GetType()
                                  .GetProperties()
                                  .Where(prop => Attribute.IsDefined(prop, typeof(MultiFormHeaderAttribute)))
                                  .Select(p=> new { Prop = p , Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, typeof(MultiFormHeaderAttribute)) } )
                                  .Select(_ =>
                                             {
                                                 var value = _.Prop.GetValue(request.Content);
                                                 var docPart = new StringContent(value.ToString());
                                                 docPart.Headers.ContentDisposition = new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) { Name = _.Attrib.Name , FileName = _.Attrib.FileName };
                                                 docPart.Headers.ContentType = new MediaTypeHeaderValue(_.Attrib.MediaType);

                                                 return docPart;
                                             }).ToList();
            var docDims = request.Dimensions
                                 .GetType()
                                 .GetProperties()
                                 .Where(prop => Attribute.IsDefined(prop, typeof(MultiFormHeaderAttribute)))
                                 .Select(p=> new { Prop = p , Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, typeof(MultiFormHeaderAttribute)) } )
                                 .Select(_ =>
                                         {
                                             var value = _.Prop.GetValue(request.Dimensions);
                                             var dimContent = new StringContent(value.ToString());
                                             dimContent.Headers.ContentDisposition = new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) { Name = _.Attrib.Name };
                                             return dimContent;
                                    }).ToList();

            docParts.AddRange(docDims);

            return docParts;
        }
    }
}