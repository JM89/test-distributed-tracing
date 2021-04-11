using Amazon.SQS.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Worker.MessageHandler
{
    public interface ISqsMessageHandler
    {
        Task<bool> ProcessMessageAsync(Message message, CancellationToken cancellationToken);
    }
}
