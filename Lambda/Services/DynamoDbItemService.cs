using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

        public async Task<bool> DoSomethingAsync(DynamodbStreamRecord record, ILambdaLogger logger)
        {
            string ParentId = "", TraceId = "", TraceParentId = "";

            logger.LogLine($"Event ID: {record.EventID}");
            logger.LogLine($"Event Name: {record.EventName}");

            using (var writer = new StringWriter())
            {
                _jsonSerializer.Serialize(writer, record.Dynamodb);

                var hasParentTrace = record.Dynamodb.NewImage.TryGetValue("ParentTraceId", out AttributeValue value);
                if (hasParentTrace)
                {
                    logger.LogLine($"ParentTraceId: {value.S}");
                    TraceParentId = value.S;
                    var traceInfo = value.S.Split("-");
                    if (traceInfo.Length == 4)
                    {
                        TraceId = traceInfo[1];
                        ParentId = traceInfo[2];
                    }
                }

                var streamRecordJson = writer.ToString();

                logger.LogLine($"DynamoDB Record:");
                logger.LogLine(streamRecordJson);
            }

            try
            {
                Activity activity = null;

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

                    logger.LogLine("Activity StartTimeUtc: " + activity?.StartTimeUtc);
                    logger.LogLine("OtlpEndpointUrl: " + _settings.DistributedTracingOptions.OtlpEndpointUrl);

                    if (!string.IsNullOrEmpty(_settings.SampleApiTwoTestEndpointUrl))
                    {
                        logger.LogLine("Calling SampleApi.Two");
                        var response = await _settings.SampleApiTwoTestEndpointUrl
                            .WithHeader("traceparent", activity.Id)
                            .GetStringAsync();
                        logger.LogLine($"SampleApi.Two called successfully: {response}");
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
                logger.LogLine($"Error occured: {ex.Message}");
                return false;
            }
        }
    }
}
