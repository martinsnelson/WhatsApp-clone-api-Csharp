# Deploy da solução LambdaApi com DynamoDB + SQS

## Pré-requisitos
- AWS CLI configurado
- SAM CLI instalado
- Dotnet 8 instalado

## Passos para Deploy

1. Compile o projeto:
   ```bash
   dotnet build src/LambdaApi

sam package \
  --template-file sam-template.yaml \
  --s3-bucket <seu-bucket-deploy> \
  --output-template-file packaged.yaml

sam deploy \
  --template-file packaged.yaml \
  --stack-name LambdaApiStack \
  --capabilities CAPABILITY_IAM

aws cloudformation describe-stacks --stack-name LambdaApiStack \
  --query "Stacks[0].Outputs" --output table

curl https://<API_ID>.execute-api.<region>.amazonaws.com/Prod/
