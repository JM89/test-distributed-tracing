using OpenTelemetry.Trace;
using System;

namespace Shared
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder ConfigureExporter(this TracerProviderBuilder builder, DistributedTracingOptions opts)
        {
            if (opts.Exporter == Exporter.ZipKin)
            {
                builder.AddZipkinExporter(opt =>
                {
                    opt.Endpoint = new Uri(opts.ZipkinEndpointUrl);
                });
            }

            if (opts.Exporter == Exporter.OtlpCollector)
            {
                var uri = new Uri(opts.OtlpEndpointUrl);

                if (uri.Scheme == "http")
                {
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                }

                builder.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = uri;
                });
            }

            return builder;
        }
    }
}
