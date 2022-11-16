aws sqs create-queue --queue-name sample-api-one --region eu-west-2 --endpoint-url http://localstack:4566

aws dynamodb create-table \
    --table-name sample-table \
    --attribute-definitions \
        AttributeName=KeyId,AttributeType=S \
    --key-schema \
        AttributeName=KeyId,KeyType=HASH \
    --provisioned-throughput \
        ReadCapacityUnits=10,WriteCapacityUnits=5 \
    --stream-specification StreamEnabled=true,StreamViewType=NEW_AND_OLD_IMAGES \
    --region eu-west-2 --endpoint-url http://localstack:4566

aws iam create-role --role-name streamer-lambda-role \
    --assume-role-policy-document file://files/lambda-trust-relationship.json \
    --region eu-west-2 --endpoint-url http://localstack:4566

aws iam put-role-policy --role-name streamer-lambda-role \
    --policy-name streamer-lambda-role-policy \
    --policy-document file://files/lambda-role-policy.json \
    --region eu-west-2 --endpoint-url http://localstack:4566

aws lambda create-function --function-name streamer-lambda \
    --zip-file fileb://files/Lambda.zip --handler MyLambda::MyLambda.Function::FunctionHandler --runtime dotnetcore3.1 \
    --role arn:aws:iam::000000000000:role/streamer-lambda-role \
    --region eu-west-2 --endpoint-url http://localstack:4566

aws lambda update-function-configuration --function-name streamer-lambda \
    --environment "Variables={Settings__DistributedTracingOptions__Exporter=OtlpCollector,Settings__DistributedTracingOptions__OtlpEndpointUrl=http://host.docker.internal:55680,Settings__SampleApiTwoTestEndpointUrl=http://host.docker.internal:5124/api/test/test2, Serilog__WriteTo__1__Args__serverUrl=http://host.docker.internal:5341}" \
    --region eu-west-2 --endpoint-url http://localstack:4566

export AWS_STREAM_NAME=`aws dynamodbstreams list-streams --region eu-west-2 --endpoint-url http://localhost:4566 --output text | awk -F"\t" '$1=="STREAMS" {print $2}'`

aws lambda create-event-source-mapping \
    --function-name streamer-lambda \
    --event-source $AWS_STREAM_NAME  \
    --batch-size 1 \
    --starting-position TRIM_HORIZON \
    --region eu-west-2 --endpoint-url http://localstack:4566

unset AWS_STREAM_NAME