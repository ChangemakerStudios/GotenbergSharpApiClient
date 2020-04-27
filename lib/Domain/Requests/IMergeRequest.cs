// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IMergeRequest: IConvertToHttpContent
    {
        int Count { get; }
        RequestConfig Config { get; set; }
    }
   
}