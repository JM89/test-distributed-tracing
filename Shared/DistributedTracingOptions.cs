namespace Shared
{
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
