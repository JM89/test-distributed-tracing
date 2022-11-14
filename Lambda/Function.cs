using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLambda.Services;
using Serilog;
using Shared;
using System.IO;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MyLambda
{
    public class Function
    {
        private readonly IDynamoDbItemService _dynamoDbItemService;

        public Function()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            configuration.LogSettings("Settings");

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(Log.Logger);
            serviceCollection.RegisterAppServices(configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _dynamoDbItemService = serviceProvider.GetRequiredService<IDynamoDbItemService>();
        }

        public async Task<string> FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            Log.Logger.Information($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records)
            {
                await _dynamoDbItemService.DoSomethingAsync(record);
            }

            Log.Logger.Information("Stream processing complete.");

            return "Success";
        }
    }
}