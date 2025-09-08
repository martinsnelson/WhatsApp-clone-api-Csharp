using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using LambdaApi.Contexts;
using LambdaApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace LambdaApi.Controllers;

[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IDynamoDBContext _IDynamoDBContext;
    private readonly IAmazonDynamoDB _IAmazonDynamoDB;
    private readonly IAmazonSQS _IAmazonSQS;
    private readonly string _deliveryQueueUrl;

    public MessagesController(IDynamoDBContext dynamoDBContext, IAmazonDynamoDB amazonDynamoDB, IAmazonSQS amazonSQS, IConfiguration conf)
    {
        _IDynamoDBContext = dynamoDBContext;
        _IAmazonDynamoDB = amazonDynamoDB;
        _IAmazonSQS = amazonSQS;
        _deliveryQueueUrl = conf["Sqs:DeliveryQueueUrl"];
    }

    /// <summary>
    /// Client cria a mensagem (idempotente)
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] MessageModel msg)
    {
        // validações mínimas
        if (msg == null || string.IsNullOrEmpty(msg.MessageId) || string.IsNullOrEmpty(msg.ConversationId))
            return BadRequest("MessageId and ConversationId required");


        // Idempotência: PutItem com ConditionExpression attribute_not_exists(MessageId)
        var item = new Dictionary<string, AttributeValue>
        {
            ["MessageId"] = new AttributeValue { S = msg.MessageId },
            ["ConversationId"] = new AttributeValue { S = msg.ConversationId },
            ["SenderId"] = new AttributeValue { S = msg.SenderId ?? string.Empty },
            ["RecipientId"] = new AttributeValue { S = msg.RecipientId ?? string.Empty },
            ["Content"] = new AttributeValue { S = msg.Content ?? string.Empty },
            ["SentAt"] = new AttributeValue { S = msg.SentAt.ToString("o") },
            ["Status"] = new AttributeValue { S = "sent" },
            ["IdempotencyKey"] = new AttributeValue { S = msg.IdempotencyKey ?? msg.MessageId }
        };


        var putReq = new PutItemRequest
        {
            TableName = "MessagesTable",
            Item = item,
            ConditionExpression = "attribute_not_exists(MessageId)"
        };


        try
        {
            await _IAmazonDynamoDB.PutItemAsync(putReq);
        }
        catch (Amazon.DynamoDBv2.Model.ConditionalCheckFailedException)
        {
            // message already exists -> fetch and return
            var existing = await _IDynamoDBContext.LoadAsync<MessageModel>(msg.MessageId);
            return Ok(existing);
        }


        // Enfileira job de entrega em SQS FIFO
        var body = JsonSerializer.Serialize(new { msg.MessageId, msg.ConversationId, msg.RecipientId });
        var sendReq = new SendMessageRequest
        {
            QueueUrl = _deliveryQueueUrl,
            MessageBody = body,
            MessageGroupId = msg.ConversationId,
            MessageDeduplicationId = msg.MessageId
        };
        await _IAmazonSQS.SendMessageAsync(sendReq);


        return Ok(msg);
    }


    /// <summary>
    /// Marca entregue com proteção contra regressão
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [HttpPost("{id}/delivered")]
    // public async Task<IActionResult> MarkDelivered(string id)
    // {
    //     var updateReq = new UpdateItemRequest
    //     {
    //         TableName = "MessagesTable",
    //         Key = new Dictionary<string, AttributeValue> { ["MessageId"] = new AttributeValue { S = id } },
    //         ExpressionAttributeNames = new Dictionary<string, string> { ["#S"] = "Status", ["#DA"] = "DeliveredAt", ["#V"] = "Version" },
    //         ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    //         {
    //             [":del"] = new AttributeValue { S = "delivered" },
    //             [":now"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") },
    //             [":one"] = new AttributeValue { N = "1" },
    //             [":read"] = new AttributeValue { S = "read" }
    //         },
    //         UpdateExpression = "SET #S = :del, #DA = :now, #V = if_not_exists(#V, :zero) + :one",
    //         ConditionExpression = "attribute_not_exists(#S) OR #S <> :read"
    //     };


    //     // add zero value used in UpdateExpression
    //     updateReq.ExpressionAttributeValues[":zero"] = new AttributeValue { N = "0" };


    //     try
    //     {
    //         await _dynamoDb.UpdateItemAsync(updateReq);
    //         return Ok();
    //     }
    //     catch (Amazon.DynamoDBv2.Model.ConditionalCheckFailedException)
    //     {
    //         // já estava em read -> ignorar
    //         return Ok();
    //     }
    // }


    /// <summary>
    /// Marca lido com controle (sem regrada para delivered->read)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [HttpPost("{id}/read")]
    // public async Task<IActionResult> MarkRead(string id)
    // {
    //     var updateReq = new UpdateItemRequest
    //     {
    //         TableName = "MessagesTable",
    //         Key = new Dictionary<string, AttributeValue> { ["MessageId"] = new AttributeValue { S = id } },
    //         ExpressionAttributeNames = new Dictionary<string, string> { ["#S"] = "Status", ["#RA"] = "ReadAt", ["#V"] = "Version" },
    //         ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    //         {
    //             [":read"] = new AttributeValue { S = "read" },
    //             [":now"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") },
    //             [":one"] = new AttributeValue { N = "1" }
    //         },
    //         UpdateExpression = "SET #S = :read, #RA = :now, #V = if_not_exists(#V, :zero) + :one",
    //         ConditionExpression = "attribute_not_exists(#S) OR #S <> :read"
    //     };

    //     updateReq.ExpressionAttributeValues[":zero"] = new AttributeValue { N = "0" };

    //     try
    //     {
    //         await _dynamoDb.UpdateItemAsync(updateReq);
    //         return Ok();
    //     }
    //     catch (Amazon.DynamoDBv2.Model.ConditionalCheckFailedException)
    //     {
    //         return Ok();
    //     }
    // }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [HttpGet("{id}")]
    // public async Task<IActionResult> GetMessage(string id)
    // {
    //     var msg = await _db.LoadAsync<Message>(id);
    //     return msg == null ? NotFound() : Ok(msg);
    // }


    /// <summary>
    /// Lista todas a mensagens
    /// </summary>
    /// <returns></returns>
    // [HttpGet("list")]
    // public async Task<IActionResult> List()
    // {
    //     var scanResponse = await _amazonDynamoDBClient.ScanAsync("MessagesTable", new List<string> { "Id", "Message", "CreatedAt" });

    //     var items = scanResponse.Items.Select(i => new
    //     {
    //         Id = i["Id"].S,
    //         Message = i["Message"].S,
    //         CreatedAt = i["CreatedAt"].S
    //     });

    //     return Ok(new { status = "listed", items });
    // }


    #region [OLD Method]
    // [HttpPost("create")]
    // public async Task<IActionResult> Create([FromBody] Message messageRequest)
    // {
    //     var item = new Dictionary<string, AttributeValue>
    //     {
    //         ["Id"] = new AttributeValue { S = Guid.NewGuid().ToString() },
    //         ["Message"] = new AttributeValue { S = messageRequest.Message ?? "empty" },
    //         ["CreatedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") }
    //     };

    //     await _amazonDynamoDBClient.PutItemAsync("MessagesTable", item);

    //     return Ok(new { status = "save", item });
    // }
    #endregion

}