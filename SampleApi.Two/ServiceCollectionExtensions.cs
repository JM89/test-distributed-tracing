using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared;
using System;
using System.Net.Http;

namespace SampleApi.Two
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("Settings").Get<Settings>();

            configuration.LogSettings("Settings");

            services.AddSingleton(settings);

            services.AddSingleton<IAmazonDynamoDB>(
                new AmazonDynamoDBClient(
                    new AmazonDynamoDBConfig()
                    {
                        ServiceURL = settings.AwsServiceUrl
                    }));

            services.RegisterOpenTelemetry(settings);

            return services;
        }

        public static IServiceCollection RegisterOpenTelemetry(this IServiceCollection services, Settings settings)
        {
            services.AddOpenTelemetryTracing((builder) =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(settings.ServiceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation(opt =>
                    {
                        opt.Filter = (httpRequestMessage) =>
                        {
                            return httpRequestMessage.Headers.Contains("X-Amz-Target");
                        };

                        opt.Enrich = (activity, eventName, rawObject) =>
                        {
                            if (eventName.Equals("OnStartActivity"))
                            {
                                if (rawObject is HttpRequestMessage request && request.Headers.Contains("X-Amz-Target"))
                                {
                                    activity.SetTag("aws.operation", string.Join(",", request.Headers.GetValues("X-Amz-Target")));
                                }
                            }
                            else if (eventName.Equals("OnException"))
                            {
                                if (rawObject is Exception exception)
                                {
                                    activity.SetTag("stackTrace", exception.StackTrace);
                                }
                            }
                        };
                    })
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
