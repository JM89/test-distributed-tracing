using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared;
using System;
using System.Diagnostics;
using Worker.MessageHandler;

namespace Worker
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
                    new AmazonSQSConfig() {
                    ServiceURL = settings.AwsServiceUrl
                }));

            services.AddSingleton<ISqsMessageHandler, SqsMessageHandler>();

            services.RegisterOpenTelemetry(settings);

            return services;
        }

        public static IServiceCollection RegisterOpenTelemetry(this IServiceCollection services, Settings settings)
        {
            services.AddSingleton(new ActivitySource("Sqs.Instrumentation"));
            services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(settings.ServiceName))
                    .AddSource("Sqs.Instrumentation")
                    .ConfigureExporter(settings.DistributedTracingOptions);
            });
            return services;
        }
    }
}
