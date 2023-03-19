using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLambda.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyLambda
{
    public static class ServiceCollectionExtensions
    {
        public static List<Activity> InMemoryActivities = new List<Activity>();

        public static IServiceCollection RegisterAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("Settings").Get<Settings>();

            services.AddSingleton(settings);
            services.AddTransient<IDynamoDbItemService, DynamoDbItemService>();

            services.RegisterOpenTelemetry(settings);

            return services;
        }

        public static IServiceCollection RegisterOpenTelemetry(this IServiceCollection services, Settings settings)
        {
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(settings.ServiceName))
                        .AddSource(settings.ServiceName)
                        .ConfigureExporter(settings.DistributedTracingOptions);
                });

            return services;
        }
    }
}
