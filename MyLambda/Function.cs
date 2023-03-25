using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLambda;
using MyLambda.Services;
using OpenTelemetry.Trace;
using Serilog;
using Shared;
using System.Diagnostics;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(CustomLambdaJsonSerializer))]

namespace MyLambda;

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
            .Enrich.FromLogContext()
            .CreateLogger();

        configuration.LogSettings("Settings");

        var serviceCollection = new ServiceCollection();

        var settings = configuration.GetSection("Settings").Get<Settings>();
        serviceCollection.AddSingleton(settings);

        serviceCollection.AddSingleton(Log.Logger);
        serviceCollection
            .AddTransient<IDynamoDbItemService, DynamoDbItemService>()
            .RegisterOpenTelemetry(settings); 

        var serviceProvider = serviceCollection.BuildServiceProvider();

        _dynamoDbItemService = serviceProvider.GetRequiredService<IDynamoDbItemService>();
    }

    /// <summary>
    /// Called by SampleApi.One
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dynamoDbItemService"></param>
    public Function(ILogger logger, IDynamoDbItemService dynamoDbItemService)
    {
        Log.Logger = logger;
        _dynamoDbItemService = dynamoDbItemService;
    }

    public async Task<string> FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        Log.Logger.Information($"Beginning to process {dynamoEvent.Records.Count} records...");

        foreach (var record in dynamoEvent.Records)
        {
            await _dynamoDbItemService.DoSomethingAsync(record);
        }

        Log.Logger.Information("Stream processing complete.");
        Log.CloseAndFlush();

        TracerProvider.Default.ForceFlush();

        return "Success";
    }
}