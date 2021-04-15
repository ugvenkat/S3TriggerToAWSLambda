resource "aws_lambda_function" "lambda_tf" {
  filename      = "./zips-ver2/S3TriggerToAWSLambda.zip"
  description = "Venkat Lambda Function"
  function_name = "S3TriggerToAWSLambda"
  role          = aws_iam_role.iam_for_lambda.arn
  timeout       = 60
  handler       = "S3TriggerToAWSLambda::S3TriggerToAWSLambda.Function::FunctionHandler"
  runtime       = "dotnetcore3.1"
  source_code_hash = filebase64sha256("./zips-ver2/S3TriggerToAWSLambda.zip")
  depends_on = [aws_iam_role.iam_for_lambda]
}

#IAM role for lambda
resource "aws_iam_role" "iam_for_lambda" {
  name = "iam_for_lambda"

  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "sts:AssumeRole",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Effect": "Allow",
      "Sid": ""
    }
  ]
}
EOF
}

#cloudwatch log group for lambda logging
resource "aws_cloudwatch_log_group" "lambda_log_group" {
  name              = "/aws/lambda/${aws_lambda_function.lambda_tf.function_name}"
  retention_in_days = 14
  depends_on = [aws_lambda_function.lambda_tf]
}

#Adding Polic for Logging and S3 Access 
resource "aws_iam_policy" "lambda_logging" {
  name = "lambda_logging"
  path = "/"
  description = "IAM policy for logging from a lambda"

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "arn:aws:logs:*:*:*",
      "Effect": "Allow"
    },
    {
      "Effect": "Allow",
      "Action": [
          "s3:*"
      ],
      "Resource": "arn:aws:s3:::*"
    }
  ]
}
EOF
}

#attach lambda iam role with lambda logging
resource "aws_iam_role_policy_attachment" "lambda_logs" {
  role = "${aws_iam_role.iam_for_lambda.name}"
  policy_arn = "${aws_iam_policy.lambda_logging.arn}"
}

#print out the lambda function properties
output "lambdafunction-details" {
  value = "aws_lambda_function.lambda_tf"
}