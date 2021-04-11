using Amazon.Lambda.Core;
using System.Threading.Tasks;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace MyLambda.Services
{
    public interface IDynamoDbItemService
    {
        Task<bool> DoSomethingAsync(DynamodbStreamRecord record, ILambdaLogger logger);
    }
}
