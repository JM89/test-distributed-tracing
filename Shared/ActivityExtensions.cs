using System.Diagnostics;

namespace Shared
{
    public static class ActivityExtensions
    {
        public static void AddSqsConsumerTags(this Activity activity, string queueName, string queueUrl)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md
            activity.AddTag("messaging.operation", "receive");
            activity.AddTag("messaging.system", "AmazonSQS");
            activity.AddTag("messaging.source", queueName);
            activity.AddTag("messaging.source_kind", "queue");
            activity.AddTag("messaging.url", queueUrl);
        }

        public static void AddSqsProducerTags(this Activity activity, string queueName, string queueUrl)
        {
            // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md
            activity.AddTag("messaging.system", "AmazonSQS");
            activity.AddTag("messaging.destination", queueName);
            activity.AddTag("messaging.destination_kind", "queue");
            activity.AddTag("messaging.url", queueUrl);
            activity.AddTag("messaging.conversation_id", activity.SpanId.ToHexString());
        }

        public static void AddDynamoDbTags(this Activity activity, string tableName, string operation)
        {
            // hhttps://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/database.md
            activity.AddTag("db.system", "dynamodb");
            activity.AddTag("db.name", tableName);
            activity.AddTag("db.operation", operation);
        }
    }
}
