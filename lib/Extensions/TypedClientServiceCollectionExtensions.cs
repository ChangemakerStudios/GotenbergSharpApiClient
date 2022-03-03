using System;
using System.Net;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class TypedClientServiceCollectionExtensions
    {
        [PublicAPI]
        public static IHttpClientBuilder AddGotenbergSharpClient(this IServiceCollection services)
        {
            return services.AddGotenbergSharpClient((sp, client) =>
            {
                var ops = GetOptions(sp);
                client.Timeout = ops.TimeOut;
                client.BaseAddress = ops.ServiceUrl;
            });
        }

        [PublicAPI]
        public static IHttpClientBuilder AddGotenbergSharpClient(this IServiceCollection services,
            Action<IServiceProvider, HttpClient> configureClient)
        {
            if (configureClient == null) throw new ArgumentNullException(nameof(configureClient));

            return services
                .AddHttpClient(nameof(GotenbergSharpClient), configureClient)
                .AddTypedClient<GotenbergSharpClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new TimeoutHandler(new HttpClientHandler
                    { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                .AddPolicyHandler(PolicyFactory.CreatePolicyFromSettings)
                .SetHandlerLifetime(TimeSpan.FromMinutes(6));
        }

        static GotenbergSharpClientOptions GetOptions(IServiceProvider sp) =>
            sp.GetRequiredService<IOptions<GotenbergSharpClientOptions>>()?.Value;
    }
}