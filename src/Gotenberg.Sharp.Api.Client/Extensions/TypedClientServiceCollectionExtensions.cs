//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System.Net;

using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;



using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gotenberg.Sharp.API.Client.Extensions;

/// <summary>
/// Extension methods for registering GotenbergSharpClient with dependency injection.
/// </summary>
public static class TypedClientServiceCollectionExtensions
{
    /// <summary>
    /// Registers GotenbergSharpClient with dependency injection using configured options.
    /// Configure options via appsettings.json or by calling services.Configure&lt;GotenbergSharpClientOptions&gt;().
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>An IHttpClientBuilder for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <remarks>
    /// This method configures the HttpClient with automatic compression, retry policies, and basic authentication if credentials are provided.
    /// Options should be configured in the "GotenbergSharpClient" section of appsettings.json or programmatically.
    /// </remarks>
    public static IHttpClientBuilder AddGotenbergSharpClient(
        this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        return services.AddGotenbergSharpClient(
            (sp, client) =>
            {
                var ops = GetOptions(sp);
                client.Timeout = ops.TimeOut;
                client.BaseAddress = ops.ServiceUrl;
            });
    }


    /// <summary>
    /// Registers GotenbergSharpClient with dependency injection using a custom HttpClient configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureClient">Action to configure the HttpClient instance.</param>
    /// <returns>An IHttpClientBuilder for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when configureClient is null.</exception>
    /// <remarks>
    /// This overload allows full control over HttpClient configuration. The client is configured with
    /// automatic compression, timeout handling, and exponential backoff retry policies.
    /// </remarks>
    public static IHttpClientBuilder AddGotenbergSharpClient(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
    {
        if (configureClient == null) throw new ArgumentNullException(nameof(configureClient));

        var builder = services
            .AddHttpClient(nameof(GotenbergSharpClient), configureClient)
            .AddTypedClient<GotenbergSharpClient>()
            .ConfigurePrimaryHttpMessageHandler(
                () => new TimeoutHandler(
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip
                                                 | DecompressionMethods.Deflate
                    }))
            .AddHttpMessageHandler(sp =>
            {
                var ops = GetOptions(sp);

                var hasUsername = !string.IsNullOrWhiteSpace(ops.BasicAuthUsername);
                var hasPassword = !string.IsNullOrWhiteSpace(ops.BasicAuthPassword);

                // Validate that both username and password are provided together
                if (hasUsername ^ hasPassword)
                {
                    throw new InvalidOperationException(
                        "BasicAuth configuration is incomplete. Both BasicAuthUsername and BasicAuthPassword must be set, or neither should be set.");
                }

                // Add basic auth handler if credentials are configured
                if (hasUsername && hasPassword)
                {
                    return new BasicAuthHandler(ops.BasicAuthUsername!, ops.BasicAuthPassword!);
                }

                // Return a pass-through handler if no auth is configured
                return new PassThroughHandler();
            })
            .AddPolicyHandler(PolicyFactory.CreatePolicyFromSettings)
            .SetHandlerLifetime(TimeSpan.FromMinutes(6));

        return builder;
    }

    private static GotenbergSharpClientOptions GetOptions(IServiceProvider sp)
    {
        return sp.GetRequiredService<IOptions<GotenbergSharpClientOptions>>().Value;
    }
}