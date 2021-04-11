using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Worker.MessageHandler;

namespace Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Settings _settings;
        private readonly IAmazonSQS _sqsClient;
        private readonly ISqsMessageHandler _sqsHandler;
        private readonly ActivitySource _activitySource;

        public Worker(ILogger<Worker> logger, Settings settings, IAmazonSQS sqsClient, ISqsMessageHandler sqsHandler, ActivitySource activitySource)
        {
            _logger = logger;
            _settings = settings;
            _sqsClient = sqsClient;
            _sqsHandler = sqsHandler;
            _activitySource = activitySource;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueUrl = $"{_settings.AwsServiceUrl}/000000000000/{_settings.QueueName}";

            while (!stoppingToken.IsCancellationRequested)
            {
                var receiveMessageRequest = new ReceiveMessageRequest()
                {
                    QueueUrl = queueUrl,
                    MessageAttributeNames = { "TraceParentId" },
                    WaitTimeSeconds = 20
                };
                
                var receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                foreach (var message in receiveMessageResponse.Messages)
                {
                    using (var activity = _activitySource.StartActivity("ReceiveMessage"))
                    {
                        // invalid parent span IDs=664fd616639bf448; skipping clock skew adjustment
                        //var hasParentTrace = message.MessageAttributes.TryGetValue("TraceParentId", out MessageAttributeValue value);
                        //if (hasParentTrace)
                        //{
                        //    // Activity.Current?.SetParentId(value.StringValue); Activity.Current is null in the Background service
                        //    activity.SetParentId(value.StringValue);
                        //}

                        using (var subActivity = _activitySource.StartActivity("ProcessMessage"))
                        {
                            await _sqsHandler.ProcessMessageAsync(message, stoppingToken);
                        }

                        using (var subActivity = _activitySource.StartActivity("DeleteMessageFromQueue"))
                        {
                            await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest()
                            {
                                QueueUrl = queueUrl,
                                ReceiptHandle = message.ReceiptHandle
                            }, stoppingToken);
                        }
                    }
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
