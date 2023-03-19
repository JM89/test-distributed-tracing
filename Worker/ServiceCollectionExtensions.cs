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

            var sqsConfig = new AmazonSQSConfig();
            if (!string.IsNullOrEmpty(settings.AwsServiceUrl))
            {
                sqsConfig.ServiceURL = settings.AwsServiceUrl;
                sqsConfig.AuthenticationRegion = settings.Region;
            }

            services.AddSingleton<IAmazonSQS>(new AmazonSQSClient(sqsConfig));

            services.AddSingleton<ISqsMessageHandler, SqsMessageHandler>();

            services.RegisterOpenTelemetry(settings);

            return services;
        }

        public static IServiceCollection RegisterOpenTelemetry(this IServiceCollection services, Settings settings)
        {
            services.AddSingleton(new ActivitySource("Sqs.Instrumentation"));
            services.AddOpenTelemetry()
                .WithTracing(builder =>
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
