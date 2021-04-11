dotnet lambda package
aws lambda update-function-code --function-name streamer-lambda \
    --zip-file fileb://bin/Release/netcoreapp3.1/Lambda.zip \
    --region eu-west-2 --endpoint-url http://localhost:4566