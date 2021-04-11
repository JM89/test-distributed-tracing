using Shared;

namespace SampleApi.Two
{
    public class Settings
    {
        public string ServiceName { get; set; }

        public string AwsServiceUrl { get; set; }

        public string TableName { get; set; }

        public DistributedTracingOptions DistributedTracingOptions { get; set; }
    }
}
