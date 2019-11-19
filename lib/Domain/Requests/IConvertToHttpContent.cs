using System.Collections.Generic;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IConvertToHttpContent
    {
        IEnumerable<HttpContent> ToHttpContent();
    }
}
