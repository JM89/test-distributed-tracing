using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared;
using System;
using System.Diagnostics;

namespace SampleApi.One
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("Settings").Get<Settings>();

            configuration.LogSettings("Settings");

            services.AddSingleton(settings);

            services.AddSingleton<IAmazonSQS>(
                new AmazonSQSClient(
                    new AmazonSQSConfig()
                    {
                        ServiceURL = settings.AwsServiceUrl
                    }));

            services.RegisterOpenTelemetry(settings);

            return services;
        }

        public static IServiceCollection RegisterOpenTelemetry(this IServiceCollection services, Settings settings)
        {
            services.AddSingleton(new ActivitySource("Sqs.Instrumentation"));
            services.AddOpenTelemetryTracing((builder) => {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(settings.ServiceName))
                    .AddAspNetCoreInstrumentation()
                    .AddSource("Sqs.Instrumentation")
                    .AddConsoleExporter();

                if (settings.DistributedTracingOptions.Exporter == Exporter.ZipKin)
                {
                    builder.AddZipkinExporter(opt =>
                    {
                        opt.Endpoint = new Uri(settings.DistributedTracingOptions.ZipkinEndpointUrl);
                    });
                }

                if (settings.DistributedTracingOptions.Exporter == Exporter.OtlpCollector)
                {
                    var uri = new Uri(settings.DistributedTracingOptions.OtlpEndpointUrl);

                    if (uri.Scheme == "http")
                    {
                        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                    }

                    builder.AddOtlpExporter(opt =>
                    {
                        opt.Endpoint = uri;
                    });
                }
            }
            );
            return services;
        }
    }
}
