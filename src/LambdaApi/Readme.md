# ASP.NET Core Web API Serverless Application

This project shows how to run an ASP.NET Core Web API project as an AWS Lambda exposed through Amazon API Gateway. The NuGet package [Amazon.Lambda.AspNetCoreServer](https://www.nuget.org/packages/Amazon.Lambda.AspNetCoreServer) contains a Lambda function that is used to translate requests from API Gateway into the ASP.NET Core framework and then the responses from ASP.NET Core back to API Gateway.


For more information about how the Amazon.Lambda.AspNetCoreServer package works and how to extend its behavior view its [README](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.AspNetCoreServer/README.md) file in GitHub.


### Configuring for API Gateway HTTP API ###

API Gateway supports the original REST API and the new HTTP API. In addition HTTP API supports 2 different
payload formats. When using the 2.0 format the base class of `LambdaEntryPoint` must be `Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction`.
For the 1.0 payload format the base class is the same as REST API which is `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction`.
**Note:** when using the `AWS::Serverless::Function` CloudFormation resource with an event type of `HttpApi` the default payload
format is 2.0 so the base class of `LambdaEntryPoint` must be `Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction`.


### Configuring for Application Load Balancer ###

To configure this project to handle requests from an Application Load Balancer instead of API Gateway change
the base class of `LambdaEntryPoint` from `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction` to 
`Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction`.

### Project Files ###

* serverless.template - an AWS CloudFormation Serverless Application Model template file for declaring your Serverless functions and other AWS resources
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
* LambdaEntryPoint.cs - class that derives from **Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction**. The code in 
this file bootstraps the ASP.NET Core hosting framework. The Lambda function is defined in the base class.
Change the base class to **Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction** when using an 
Application Load Balancer.
* LocalEntryPoint.cs - for local development this contains the executable Main function which bootstraps the ASP.NET Core hosting framework with Kestrel, as for typical ASP.NET Core applications.
* Startup.cs - usual ASP.NET Core Startup class used to configure the services ASP.NET Core will use.
* appsettings.json - used for local development.
* Controllers\ValuesController - example Web API controller

You may also have a test project depending on the options selected.

## Here are some steps to follow from Visual Studio:

To deploy your Serverless application, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed application open the Stack View window by double-clicking the stack name shown beneath the AWS CloudFormation node in the AWS Explorer tree. The Stack View also displays the root URL to your published application.

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "LambdaApi/test/LambdaApi.Tests"
    dotnet test
```

Deploy application
```
    cd "LambdaApi/src/LambdaApi"
    dotnet lambda deploy-serverless
```

# Helps:
Create AWS IAM User:
1. Login console AWS or create account.
2. Acesse AWS service IAM -> Users -> Create user.
3. Create user
4. Define user
5. Select Add user to group -> Next
6. Create user
7. Acesse User -> Security credentials -> Access keys -> Create access key -> Command Line Interface (CLI) -> Create access key.

Copy Key and Secret:
1. Copy AWS Access Key ID and AWS Secret Access Key.

Create User groups:
1. Create group
2. Permissions -> Add permissions -> AWSLambda_FullAccess, AmazonDynamoDBFullAccess
3. In user create -> Groups -> Add user to groups -> Add Group to user
5. Add permissions
    AmazonDynamoDBFullAccess
    AmazonS3FullAccess
    AWSCloudFormationFullAccess
    AWSLambda_FullAccess

Configure AWS local machine:
1. aws configure
2. AWS_Access_Key_ID
3. AWS_Secret_Access_Key
4. sa-east-1
5. yaml
6. Teste:aws sts get-caller-identity
    Account: '89XXXXXXXXXX'
    Arn: arn:aws:iam::89XXXXXXXXXX:user/UserCriado

Create table DynamoDB:
aws dynamodb create-table \
    --table-name MessagesTable \
    --attribute-definitions AttributeName=Id,AttributeType=S \
    --key-schema AttributeName=Id,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST

Create bucket S3 para deploy:
aws s3 mb s3://deploy-whatsapp-bucket --region sa-east-1

Tools to Deploy da Lambda:
dotnet tool install -g Amazon.Lambda.Tools

Deploy da Lambda:
1. dotnet lambda deploy-serverless
2. Enter CloudFormation Stack Name: cf-deploy-whatsapp
3. cd "LambdaApi/src/LambdaApi": dotnet lambda deploy-serverless
4. Enter S3 Bucket: deploy-whatsapp-bucket

Install templates:
dotnet new install Amazon.Lambda.Templates

Create project:
dotnet new serverless.AspNetCoreWebAPI --name LambdaApi

Packages Add:
dotnet add package AWSSDK.DynamoDBv2

Run APP with SLN:
1. dotnet restore LambdaApi.sln
2. dotnet build LambdaApi.sln
3. cd folder API
4. dotnet run --project LambdaApi.csproj

