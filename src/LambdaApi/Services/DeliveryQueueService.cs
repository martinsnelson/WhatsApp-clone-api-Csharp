using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace LambdaApi.Services;


public class DeliveryQueueService
{
    private readonly IAmazonSQS _IAmazonSQS;
    private readonly string _queueUrl;

    public DeliveryQueueService(IAmazonSQS amazonSQS, IConfiguration conf)
    {
        _IAmazonSQS = amazonSQS;
        _queueUrl = conf["DeliveryQueueUrl"];
    }


    public async Task EnqueueDelivery(string messageId, string conversationId, string recipientId)
    {
        var body = JsonSerializer.Serialize(new { messageId, conversationId, recipientId });
        var req = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = body,
            MessageGroupId = conversationId,
            MessageDeduplicationId = messageId
        };
        await _IAmazonSQS.SendMessageAsync(req);
    }
}