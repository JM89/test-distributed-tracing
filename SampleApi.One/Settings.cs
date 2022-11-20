using Shared;

namespace SampleApi.One
{
    public class Settings
    {
        public string ServiceName { get; set; }

        public string AwsServiceUrl { get; set; }

        public string QueueName { get; set; }

        public string Region { get; set; }

        public string SampleApiTwoTestEndpointUrl { get; set; }

        public DistributedTracingOptions DistributedTracingOptions { get; set; }
    }
}
