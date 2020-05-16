using System;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain
{
    public class GotenbergSharpClientOptions
    {
        [UsedImplicitly]
        public Uri ServiceUrl { get; [UsedImplicitly] set; } = new Uri("http://localhost:3000");

        [UsedImplicitly]
        public Uri HealthCheckUrl { get; [UsedImplicitly] set; } = new Uri("http://localhost:3000/ping");

        [UsedImplicitly]
        public int PollyRetryCount { get; set; } = 3;

        [UsedImplicitly]
        public bool LogPollyRetries { get; set; }
    }
}