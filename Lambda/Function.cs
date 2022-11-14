using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLambda.Services;
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

            configuration.LogSettings("Settings");

            var serviceCollection = new ServiceCollection();

            serviceCollection.RegisterAppServices(configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _dynamoDbItemService = serviceProvider.GetRequiredService<IDynamoDbItemService>();
        }

        public async Task<string> FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records)
            {
                await _dynamoDbItemService.DoSomethingAsync(record, context.Logger);
            }

            context.Logger.LogLine("Stream processing complete.");

            return "Success";
        }
    }
}