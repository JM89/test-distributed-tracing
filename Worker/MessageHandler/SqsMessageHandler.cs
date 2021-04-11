using Amazon.SQS.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Worker.MessageHandler
{
    public class SqsMessageHandler : ISqsMessageHandler
    {
        public async Task<bool> ProcessMessageAsync(Message message, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Read Message {message.Body}");

            await Task.Delay(1000, cancellationToken);

            return true;
        }
    }
}
