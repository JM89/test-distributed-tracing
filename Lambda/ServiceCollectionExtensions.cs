using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLambda.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared;
using System.Diagnostics;

namespace MyLambda
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("Settings").Get<Settings>();

            configuration.LogSettings("Settings");

            services.AddSingleton(settings);
            services.AddTransient<IDynamoDbItemService, DynamoDbItemService>();

            services.RegisterOpenTelemetry(settings);

            return services;
        }

        public static IServiceCollection RegisterOpenTelemetry(this IServiceCollection services, Settings settings)
        {
            services.AddSingleton(new ActivitySource("Flurl.Instrumentation"));
            services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(settings.ServiceName))
                    .AddSource("Flurl.Instrumentation")
                    .ConfigureExporter(settings.DistributedTracingOptions)
                    .Build(); 
            });
            return services;
        }
    }
}
