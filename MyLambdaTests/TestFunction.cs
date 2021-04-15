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
                            
                        }
                    }
                }
            };
            var function = new MyLambda.Function();
            await function.FunctionHandler(dynamoDbEvent, _mockLambdaContext.Object);
        }
    }
}
