using Amazon.SQS;
using Amazon.SQS.Model;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApi.One.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly ActivitySource _activitySource;
        private readonly Settings _settings;

        public TestController(IAmazonSQS sqsClient, ActivitySource activitySource, Settings settings)
        {
            _sqsClient = sqsClient;
            _activitySource = activitySource;
            _settings = settings;
        }

        /// <summary>
        /// https://localhost:5001/api/test/test
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
                    Console.WriteLine("Calling SampleApi.Two");
                    var response = await _settings
                        .SampleApiTwoTestEndpointUrl
                        .WithHeader("traceparent", Activity.Current?.Id)
                        .GetStringAsync();
                    Console.WriteLine($"SampleApi.Two called successfully: {response}");
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
    }
}
