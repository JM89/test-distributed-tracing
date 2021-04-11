using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
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

        public TestController(Settings settings, IAmazonDynamoDB dynamoDbClient)
        {
            _settings = settings;
            _dynamoDbClient = dynamoDbClient;
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
                var request = new PutItemRequest
                {
                    TableName = _settings.TableName,
                    Item = new Dictionary<string, AttributeValue>()
                    {
                        { "KeyId", new AttributeValue { S = Guid.NewGuid().ToString() } },
                        { "ParentTraceId", new AttributeValue { S = Activity.Current?.ParentId ?? "None" } },
                        { "Payload", new AttributeValue { S = "{\"important-msg\": \"helloworld\"}" } }
                    }
                };
                await _dynamoDbClient.PutItemAsync(request, ct);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(ex);
            }

            return new OkObjectResult($"Entry saved to table { _settings.TableName }");
        }
    }
}
