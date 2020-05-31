using System;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Settings
{
    public class GotenbergSharpClientOptions
    {
        [UsedImplicitly]
        public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(3);

        [UsedImplicitly]
        public Uri ServiceUrl { get; set; } = new Uri("http://localhost:3000");

        [UsedImplicitly]
        public Uri HealthCheckUrl { get; set; } = new Uri("http://localhost:3000/ping");

        [UsedImplicitly]
        public RetryOptions RetryPolicy { get; set; } = new RetryOptions();
    }
}