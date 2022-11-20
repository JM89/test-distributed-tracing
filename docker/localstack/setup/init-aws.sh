#!/bin/sh

aws s3api create-bucket --bucket terraform-state --endpoint-url http://localstack:4566 --region eu-west-2 --create-bucket-configuration LocationConstraint=eu-west-2


# shellcheck disable=SC2164
cd "/etc/localstack/init/ready.d/terraform"

echo "terraform init -reconfigure"
terraform init -reconfigure

echo "terraform plan -var-file="./envs/$TERRAFORM_ENVIRONMENT.tfvars" -out plan"
terraform plan -var-file="./envs/$TERRAFORM_ENVIRONMENT.tfvars" -out plan

echo "terraform apply plan"
terraform apply plan