using Shared;

namespace Worker
{
    public class Settings
    {
        public string ServiceName { get; set; }

        public string AwsServiceUrl { get; set; }

        public string QueueName { get; set; }

        public DistributedTracingOptions DistributedTracingOptions { get; set; }
    }
}
