using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain
{
    public class RetryOptions
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        [UsedImplicitly]
        public int RetryCount { get; set; } = 3;

        [UsedImplicitly]
        public double BackoffPower { get; set; } = 1.5;

        [UsedImplicitly]
        public bool LoggingEnabled { get; set; } = true;
    }
}