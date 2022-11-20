using Shared;

namespace MyLambda
{
    public class Settings
    {
        public string ServiceName { get; set; }

        public string SampleApiTwoTestEndpointUrl { get; set; }

        public DistributedTracingOptions DistributedTracingOptions { get; set; }
    }
}
