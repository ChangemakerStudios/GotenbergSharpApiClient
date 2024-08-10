

namespace Gotenberg.Sharp.API.Client.Domain.Settings
{
    public class RetryOptions
    {
        
        public bool Enabled { get; set; }

        
        public int RetryCount { get; set; } = 3;

        /// <summary>
        ///  Configures the sleep duration provider with an exponential wait time between retries. 
        ///  E.G. sleepDurationProvider: retryCount => TimeSpan.FromSeconds(Math.Pow(retryOps.BackoffPower, retryCount))
        /// </summary>
        
        public double BackoffPower { get; set; } = 1.5;

        
        public bool LoggingEnabled { get; set; } = true;
    }
}