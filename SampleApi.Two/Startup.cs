using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace SampleApi.Two
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Console.WriteLine("Starting API");
            _configuration = configuration;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var settings = _configuration.GetSection("Settings").Get<Settings>();

            services.RegisterAppServices(settings);
            services.RegisterOpenTelemetry(settings);
            services.AddSingleton(Log.Logger);
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStopping.Register(OnShutdown);

            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }

        private void OnShutdown()
        {
            Console.WriteLine("Stopping API");
        }
    }
}
