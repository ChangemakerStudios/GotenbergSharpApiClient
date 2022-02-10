using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Extensions;

public static class ValidOfficeMergeItemExtensions
{
    internal static IEnumerable<HttpContent> ToHttpContent(this IEnumerable<ValidOfficeMergeItem> validItems)
    {
        foreach (var item in validItems)
        {
            var contentItem = item.Asset.Value.ToHttpContentItem();

            contentItem.Headers.ContentDisposition =
                new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
                {
                    Name = Constants.Gotenberg.FormFieldNames.Files,
                    FileName = item.Asset.Key
                };

            contentItem.Headers.ContentType = new MediaTypeHeaderValue(item.MediaType);

            yield return contentItem;
        }
    }
}