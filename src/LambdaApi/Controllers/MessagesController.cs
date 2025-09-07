using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LambdaApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace LambdaApi.Controllers;

[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly AmazonDynamoDBClient _amazonDynamoDBClient; // TODO - Adicionar repository para melhor separação

    public MessagesController()
    {
        _amazonDynamoDBClient = new AmazonDynamoDBClient(); // TODO - Injetar via interface
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MessageRequest messageRequest)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = Guid.NewGuid().ToString() },
            ["Message"] = new AttributeValue { S = messageRequest.Message ?? "empty" },
            ["CreatedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") }
        };

        await _amazonDynamoDBClient.PutItemAsync("MessagesTable", item);

        return Ok(new { status = "save", item });
    }

    [HttpGet("list")]
    public async Task<IActionResult> List()
    {
        var scanResponse = await _amazonDynamoDBClient.ScanAsync("MessagesTable", new List<string> { "Id", "Message", "CreatedAt" });

        var items = scanResponse.Items.Select(i => new
        {
            Id = i["Id"].S,
            Message = i["Message"].S,
            CreatedAt = i["CreatedAt"].S
        });

        return Ok(new { status = "listed", items });
    }

}