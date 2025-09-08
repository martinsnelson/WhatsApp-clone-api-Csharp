// using System.Text.Json;
// using Amazon.DynamoDBv2;
// using Amazon.DynamoDBv2.Model;
// using Amazon.Lambda.Core;
// using Amazon.Lambda.SQSEvents;
// using LambdaApi.Contexts;

// [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

// namespace LambdaApi.Workers;

// public class SqsDeliveryWorker
// {
//     private readonly IDynamoDBContext _IDynamoDBContext;
//     private readonly IAmazonDynamoDB _IAmazonDynamoDB;
//     public SqsDeliveryWorker(IDynamoDBContext dynamoDBContext, IAmazonDynamoDB amazonDynamoDB)
//     {
//         _IDynamoDBContext = dynamoDBContext;
//         _IAmazonDynamoDB = amazonDynamoDB;
//     }

//     public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
//     {
//         foreach (var record in sqsEvent.Records)
//         {
//             var body = JsonSerializer.Deserialize<Dictionary<string, string>>(record.Body);
//             var messageId = body["MessageId"];

//             // Simulação de entrega
//             context.Logger.LogInformation($"[DeliveryWorker] Entregando mensagem {messageId}");

//             var updateReq = new UpdateItemRequest
//             {
//                 TableName = "MessagesTable",
//                 Key = new Dictionary<string, AttributeValue>
//                 {
//                     ["MessageId"] = new AttributeValue { S = messageId }
//                 },
//                 UpdateExpression = "SET #S = :del, DeliveredAt = :now ADD DeliveryAttempts :one",
//                 ExpressionAttributeNames = new Dictionary<string, string> { ["#S"] = "Status" },
//                 ExpressionAttributeValues = new Dictionary<string, AttributeValue>
//                 {
//                     [":del"] = new AttributeValue { S = "delivered" },
//                     [":now"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") },
//                     [":one"] = new AttributeValue { N = "1" }
//                 }
//             };

//             await _IAmazonDynamoDB.UpdateItemAsync(updateReq);
//         }
//     }
// }