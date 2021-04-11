using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace SampleApi.Two
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults((webHostBuilder) =>
                {
                    webHostBuilder.ConfigureKestrel(x =>
                    {
                        x.AddServerHeader = false;
                    })
                    .UseStartup<Startup>();
                });
    }
}
