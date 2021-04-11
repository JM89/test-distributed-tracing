namespace SampleApi.Two
{
    public class Settings
    {
        public string ServiceName { get; set; }

        public string AwsServiceUrl { get; set; }

        public string TableName { get; set; }

        public DistributedTracingOptions DistributedTracingOptions { get; set; }
    }

    public class DistributedTracingOptions
    {
        public Exporter Exporter { get; set; }

        public string ZipkinEndpointUrl { get; set; } = "";

        public string OtlpEndpointUrl { get; set; } = "";
    }

    public enum Exporter
    {
        ZipKin, OtlpCollector
    }
}
