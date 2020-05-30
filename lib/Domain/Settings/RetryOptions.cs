using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Settings
{
    public class RetryOptions
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        [UsedImplicitly]
        public int RetryCount { get; set; } = 3;

        /// <summary>
        ///  Configures the sleep duration provider with an exponential wait time backoff between retries. Based on the current retry attempt.
        ///  E.G. sleepDurationProvider: retryCount => TimeSpan.FromSeconds(Math.Pow(retryOps.BackoffPower, retryCount))
        /// </summary>
        [UsedImplicitly]
        public double BackoffPower { get; set; } = 1.5;

        [UsedImplicitly]
        public bool LoggingEnabled { get; set; } = true;
    }
}