using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Moq;
using MyLambda;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace MyLambdaTests
{
    public class TestFunction
    {
        private readonly Mock<ILambdaContext> _mockLambdaContext;
        private readonly Mock<ILambdaLogger> _mockLambdaLogger;

        public TestFunction() {
            _mockLambdaContext = new Mock<ILambdaContext>();
            _mockLambdaLogger = new Mock<ILambdaLogger>();
            _mockLambdaLogger.Setup(x => x.LogLine(It.IsAny<string>()));
            _mockLambdaContext.Setup(x => x.Logger).Returns(_mockLambdaLogger.Object);
        }

        [Fact]
        public async Task Trigger_Function()
        {
            var dynamoDbEvent = new DynamoDBEvent() { 
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
            var function = new MyLambda.Function();
            var result = await function.FunctionHandler(dynamoDbEvent, _mockLambdaContext.Object);

            Assert.Equal("Success", result);
        }
    }
}
