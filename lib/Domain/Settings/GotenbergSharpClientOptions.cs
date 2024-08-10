

namespace Gotenberg.Sharp.API.Client.Domain.Settings
{
    public class GotenbergSharpClientOptions
    {
        
        public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(3);

        
        public Uri ServiceUrl { get; set; } = new Uri("http://localhost:3000");

        
        public Uri HealthCheckUrl { get; set; } = new Uri("http://localhost:3000/health");

        
        public RetryOptions RetryPolicy { get; set; } = new RetryOptions();
    }
}