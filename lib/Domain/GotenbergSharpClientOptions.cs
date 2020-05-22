using System;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain
{
    public class GotenbergSharpClientOptions
    {
        [UsedImplicitly]
        public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(3);

        [UsedImplicitly]
        public Uri ServiceUrl { get; [UsedImplicitly] set; } = new Uri("http://localhost:3000");

        [UsedImplicitly]
        public Uri HealthCheckUrl { get; [UsedImplicitly] set; } = new Uri("http://localhost:3000/ping");

        [UsedImplicitly]
        public int RetryCount { get; set; } = 3;

        [UsedImplicitly]
        public bool RetryOnFailure { get; set; } = true;

        [UsedImplicitly]
        public bool LogRetries { get; set; }
    }
}