using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace SampleApi.One.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly ActivitySource _activitySource;
        private readonly Settings _settings;
        private readonly ILogger _logger;

        public TestController(IAmazonSQS sqsClient, ActivitySource activitySource, Settings settings, ILogger logger)
        {
            _sqsClient = sqsClient;
            _activitySource = activitySource;
            _settings = settings;
            _logger = logger;
        }

        /// <summary>
        /// /api/test/test
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("test")]
        public async Task<IActionResult> TestAsync(CancellationToken ct = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(_settings.SampleApiTwoTestEndpointUrl))
                {
                    _logger.Information("Calling SampleApi.Two");
                    var response = await _settings
                        .SampleApiTwoTestEndpointUrl
                        .WithHeader("traceparent", Activity.Current?.Id)
                        .GetStringAsync();
                    _logger.Information($"SampleApi.Two called successfully: {response}");
                }

            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }

            try
            {
                var queueUrl = $"{_settings.AwsServiceUrl}/000000000000/{_settings.QueueName}";

                using (var activity = _activitySource.StartActivity("send", ActivityKind.Producer))
                {
                    activity.AddSqsProducerTags(_settings.QueueName, queueUrl);

                    var msg = new SendMessageRequest()
                    {
                        QueueUrl = queueUrl,
                        MessageBody = $"Helloworld from {_settings.ServiceName}",
                        MessageAttributes = new Dictionary<string, MessageAttributeValue>() {
                        {
                            "TraceParentId", new MessageAttributeValue() { DataType = "String", StringValue = activity.Id ?? "None" } }
                        }
                    };

                    var response = await _sqsClient.SendMessageAsync(msg, ct);
                    activity.AddTag("status.code", response.HttpStatusCode);
                }
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }

            return new OkObjectResult($"Message sent To {_settings.QueueName}");
        }

        /// <summary>
        /// /api/test/test-lambda
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("test-lambda")]
        public async Task<IActionResult> TestLambda(CancellationToken ct = default)
        {
            try
            {
                var dynamoDbEvent = new DynamoDBEvent()
                {
                    Records = new List<DynamodbStreamRecord>()
                {
                    new DynamodbStreamRecord()
                    {
                        EventID = "ID",
                        EventName = "Name",
                        Dynamodb = new Amazon.DynamoDBv2.Model.StreamRecord()
                        {
                            NewImage = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>()
                            {
                                {"ParentTraceId" , new Amazon.DynamoDBv2.Model.AttributeValue() { S = "00-6354617bdf314d4ebd3fb4cffb641aab-719b9c4acd868142-01" } }
                            }
                        }
                    }
                }
                };

                var function = new MyLambda.Function(_logger);

                var result = await function.FunctionHandler(dynamoDbEvent, new LambdaContextStub());
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
            }

            return new OkObjectResult($"Lambda invoked");
        }
    }

    public class LambdaContextStub : ILambdaContext
    {
        public string AwsRequestId => default(string);

        public IClientContext ClientContext => default(IClientContext);

        public string FunctionName => default(string);

        public string FunctionVersion => default(string);

        public ICognitoIdentity Identity => default(ICognitoIdentity);

        public string InvokedFunctionArn => default(string);

        public ILambdaLogger Logger => default(ILambdaLogger);

        public string LogGroupName => default(string);

        public string LogStreamName => default(string);

        public int MemoryLimitInMB => default(int);

        public TimeSpan RemainingTime => default(TimeSpan);
    }
}
