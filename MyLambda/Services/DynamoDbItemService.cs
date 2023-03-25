using Flurl.Http;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using SerilogTimings;
using System.Diagnostics;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace MyLambda.Services
{
    public class DynamoDbItemService : IDynamoDbItemService
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly Settings _settings;
        private readonly ActivitySource _activitySource;

        public DynamoDbItemService(Settings settings, ActivitySource activitySource)
        {
            _jsonSerializer = new JsonSerializer();
            _settings = settings;
            _activitySource = activitySource;
        }

        public async Task<bool> DoSomethingAsync(DynamodbStreamRecord record)
        {
            string ParentId = "", TraceId = "", TraceParentId = "";

            using (LogContext.PushProperty("Event ID", record.EventID))
            using (LogContext.PushProperty("Event Name", record.EventName))
            {
                using (Operation.Time("Do something with dynamodb record"))
                {
                    using (var writer = new StringWriter())
                    {
                        _jsonSerializer.Serialize(writer, record.Dynamodb);

                        var hasParentTrace = record.Dynamodb.NewImage.TryGetValue("ParentTraceId", value: out var value);
                        if (hasParentTrace && value != null)
                        {
                            Log.Logger.Information($"ParentTraceId found: {value.S}");

                            TraceParentId = value.S;
                            var traceInfo = value.S.Split("-");
                            if (traceInfo.Length == 4)
                            {
                                TraceId = traceInfo[1];
                                ParentId = traceInfo[2];
                            }
                        }

                        var streamRecordJson = writer.ToString();
                    }

                    try
                    {
                        Activity? activity = null;

                        try
                        {
                            if (!string.IsNullOrEmpty(TraceParentId))
                            {
                                activity = _activitySource.StartActivity("call-api", ActivityKind.Client,
                                    TraceParentId, startTime: DateTimeOffset.UtcNow);
                            }
                            else
                            {
                                activity = _activitySource.StartActivity("call-api", ActivityKind.Client);
                            }

                            using (LogContext.PushProperty("Activity StartTimeUtc", activity?.StartTimeUtc))
                            using (LogContext.PushProperty("OtlpEndpointUrl", _settings.DistributedTracingOptions.OtlpEndpointUrl))
                            using (LogContext.PushProperty("TraceId", TraceId))
                            using (LogContext.PushProperty("ParentId", ParentId))
                            {
                                Log.Logger.Information("Trigger call to SampleApi.Two");
                                using (Operation.Time("Call SampleApi.Two"))
                                {
                                    if (!string.IsNullOrEmpty(_settings.SampleApiTwoTestEndpointUrl))
                                    {
                                        var response = await _settings.SampleApiTwoTestEndpointUrl
                                           .WithHeader("traceparent", activity?.Id)
                                           .GetStringAsync();
                                    }
                                }
                            }
                            return true;
                        }
                        finally
                        {
                            activity?.Stop();
                            activity?.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Error occured {message}", ex.Message);
                        return false;
                    }
                }
            }
        }
    }
}
