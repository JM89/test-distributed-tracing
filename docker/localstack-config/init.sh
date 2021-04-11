aws sqs create-queue --queue-name sample-api-one --region eu-west-2 --endpoint-url http://localstack:4566

aws dynamodb create-table \
    --table-name sample-table \
    --attribute-definitions \
        AttributeName=KeyId,AttributeType=S \
    --key-schema \
        AttributeName=KeyId,KeyType=HASH \
    --provisioned-throughput \
        ReadCapacityUnits=10,WriteCapacityUnits=5 \
    --region eu-west-2 --endpoint-url http://localstack:4566