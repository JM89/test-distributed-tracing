using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared;
using System.Diagnostics;

namespace MyLambda
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterOpenTelemetry(this IServiceCollection services, Settings settings)
        {
            services.AddSingleton(new ActivitySource(settings.ServiceName));
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
