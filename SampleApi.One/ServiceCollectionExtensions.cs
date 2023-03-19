using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared;
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

            var sqsConfig = new AmazonSQSConfig();
            if (!string.IsNullOrEmpty(settings.AwsServiceUrl))
            {
                sqsConfig.ServiceURL = settings.AwsServiceUrl;
                sqsConfig.AuthenticationRegion = settings.Region;
            }

            services.AddSingleton<IAmazonSQS>(new AmazonSQSClient(sqsConfig));

            services.RegisterOpenTelemetry(settings);

            return services;
        }

        public static IServiceCollection RegisterOpenTelemetry(this IServiceCollection services, Settings settings)
        {
            services.AddSingleton(new ActivitySource("Sqs.Instrumentation"));
            services.AddOpenTelemetry()
                .WithTracing(builder => {
                    builder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(settings.ServiceName))
                        .AddAspNetCoreInstrumentation()
                        .AddSource("Sqs.Instrumentation")
                        .AddSource("Flurl.Instrumentation")
                        .AddSource(settings.ServiceName)
                        .ConfigureExporter(settings.DistributedTracingOptions);
                });
            return services;
        }
    }
}
