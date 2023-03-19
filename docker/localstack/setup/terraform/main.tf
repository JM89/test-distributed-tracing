resource "aws_sqs_queue" "sample_api_one" {
  count = var.create_resources ? 1 : 0
  name  = "sample-api-one"
  tags  = local.default_tags
}

resource "aws_dynamodb_table" "sample_table" {
  count = var.create_resources ? 1 : 0
  name  = "sample-table"
  attribute {
    name = "KeyId"
    type = "S"
  }
  hash_key         = "KeyId"
  read_capacity    = 10
  write_capacity   = 5
  stream_enabled   = true
  stream_view_type = "NEW_AND_OLD_IMAGES"
  tags             = local.default_tags
}

resource "aws_iam_role" "streamer_lambda_role" {
  count              = var.create_resources ? 1 : 0
  name               = "streamer-lambda-role"
  assume_role_policy = data.aws_iam_policy_document.streamer_lambda_role_trust_policy[0].json
  tags               = local.default_tags
}

data "aws_iam_policy_document" "streamer_lambda_role_trust_policy" {
  count = var.create_resources ? 1 : 0
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      identifiers = ["lambda.amazonaws.com"]
      type        = "Service"
    }
  }
}

resource "aws_iam_role_policy" "stream_lambda_role_policy" {
  policy = data.aws_iam_policy_document.stream_lambda_role_policy[0].json
  role   = aws_iam_role.streamer_lambda_role[0].id
}

data "aws_iam_policy_document" "stream_lambda_role_policy" {
  count = var.create_resources ? 1 : 0
  statement {
    actions = [
      "logs:CreateLogGroup",
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = [
      "arn:aws:logs:eu-west-2:000000000000:*"
    ]
  }
  statement {
    actions = [
      "dynamodb:DescribeStream",
      "dynamodb:GetRecords",
      "dynamodb:GetShardIterator",
      "dynamodb:ListStreams"
    ]
    resources = [
      aws_dynamodb_table.sample_table[0].stream_arn
    ]
  }
}

resource "aws_lambda_function" "streamer_lambda" {
  count            = var.create_resources ? 1 : 0
  function_name    = "streamer-lambda"
  handler          = "MyLambda::MyLambda.Function::FunctionHandler"
  role             = aws_iam_role.streamer_lambda_role[0].arn
  runtime          = "dotnet6"
  filename         = "./resources/MyLambda.zip"
  source_code_hash = filebase64sha256("./resources/MyLambda.zip")
  tags             = local.default_tags
  environment {
    variables = {
      "Settings__DistributedTracingOptions__Exporter" : "OtlpCollector",
      "Settings__DistributedTracingOptions__OtlpEndpointUrl" : "http://host.docker.internal:4318",
      "Settings__SampleApiTwoTestEndpointUrl" : "http://host.docker.internal:5124/api/test/test2",
      "Serilog__WriteTo__1__Args__serverUrl" : "http://host.docker.internal:5341"
    }
  }
}

resource "aws_sqs_queue" "streamer_lambda_dlq" {
  count = var.create_resources ? 1 : 0
  name  = "streamer-lambda-dlq"
  tags  = local.default_tags
}

resource "aws_lambda_event_source_mapping" "streamer_lambda_event_source" {
  count             = var.create_resources ? 1 : 0
  function_name     = aws_lambda_function.streamer_lambda[0].function_name
  event_source_arn  = aws_dynamodb_table.sample_table[0].stream_arn
  batch_size        = 1
  starting_position = "TRIM_HORIZON"
  destination_config {
    on_failure {
      destination_arn = aws_sqs_queue.streamer_lambda_dlq[0].arn
    }
  }
}
