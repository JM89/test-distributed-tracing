dotnet lambda package
aws lambda update-function-code --function-name streamer-lambda \
    --zip-file fileb://bin/Release/net6.0/MyLambda.zip \
    --region eu-west-2 --endpoint-url http://localhost:4566
$SHELL