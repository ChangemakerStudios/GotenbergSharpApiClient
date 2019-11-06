// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.StaticFiles;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class PdfRequestExtensions
    {
        /// <summary>
        /// Transforms the specified request into a collection of <see cref="HttpContent" /> items
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public static IReadOnlyList<HttpContent> ToHttpContentCollection(this PdfRequest request)
        {
            var docBody = ToHttpContentCollection(request, request.Content.GetType());
            var dimensions = ToHttpContentCollection(request, request.Dimensions.GetType());
            docBody.AddRange(dimensions);

            return docBody.AsReadOnly();
        }

        /// <summary>
        /// Adds assets (images, css) to the content collection.
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>See https://thecodingmachine.github.io/gotenberg/#html.assets </remarks>
        /// <returns></returns>
        public static IReadOnlyList<ByteArrayContent> AddAssetsToHttpContentCollection(this PdfRequest request)
        {
            return request.Assets?
                          .Select(item =>
                                  {
                                      new FileExtensionContentTypeProvider().TryGetContentType(item.Key, out var contentType);
                                      return new { Asset = item, ContentType = contentType };

                                  })
                          .Where(_ => _.ContentType.IsSet())
                          .Select(item =>
                                  {
                                      var assetItem = new ByteArrayContent(item.Asset.Value);
                                      assetItem.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "files", FileName = item .Asset.Key };
                                      assetItem.Headers.ContentType = new MediaTypeHeaderValue(item.ContentType);
                                      return assetItem;
                                  })
                          .ToList();
        }


        static List<StringContent> ToHttpContentCollection(PdfRequest request, Type type)
        {
            var multiType = typeof(MultiFormHeaderAttribute);

            return type.GetProperties()
                       .Where(prop => Attribute.IsDefined(prop, multiType))
                       .Select(p=> new { Prop = p , Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, multiType) } )
                       .Select(_ =>
                               {
                                   var isForContent = type == typeof(DocumentContent);
                                   var fileName = isForContent ? _.Attrib.FileName : null;

                                   var value = _.Prop.GetValue( isForContent ? request.Content :  (object)request.Dimensions);
                                   var contentItem = new StringContent(value.ToString());
                                   contentItem.Headers.ContentDisposition = new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) {Name = _.Attrib.Name, FileName = fileName};

                                   if (isForContent)
                                   {
                                       contentItem.Headers.ContentType = new MediaTypeHeaderValue(_.Attrib.MediaType);
                                   }

                                   return contentItem;
                               }).ToList();
        }
    }
}