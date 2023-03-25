using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using SerilogTimings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApi.Two.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly Settings _settings;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly ILogger _logger;

        public TestController(Settings settings, IAmazonDynamoDB dynamoDbClient, ILogger logger)
        {
            _settings = settings;
            _dynamoDbClient = dynamoDbClient;
            _logger = logger;
        }

        /// <summary>
        /// https://localhost:5003/api/test/test
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("test")]
        public async Task<IActionResult> TestAsync(CancellationToken ct = default)
        {
            try
            {
                using (LogContext.PushProperty("Activity Id", Activity.Current?.Id))
                {
                    Log.Logger.Information("Trigger call to DynamoDB");
                    using (Operation.Time("Call DynamoDB"))
                    {
                        var request = new PutItemRequest
                        {
                            TableName = _settings.TableName,
                            Item = new Dictionary<string, AttributeValue>()
                            {
                                { "KeyId", new AttributeValue { S = Guid.NewGuid().ToString() } },
                                { "ParentTraceId", new AttributeValue { S = Activity.Current?.Id ?? "None" } },
                                { "Payload", new AttributeValue { S = "{\"important-msg\": \"helloworld\"}" } }
                            }
                        };
                        await _dynamoDbClient.PutItemAsync(request, ct);
                    }
                }
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }

            return new OkObjectResult($"Entry saved to table { _settings.TableName }");
        }

        /// <summary>
        /// https://localhost:5003/api/test/test2
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("test2")]
        public IActionResult Test2Async(CancellationToken ct = default)
        {
            return new OkObjectResult($"Validated!");
        }
    }
}
