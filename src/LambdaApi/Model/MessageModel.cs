using Amazon.DynamoDBv2.DataModel;

namespace LambdaApi.Model;

[DynamoDBTable("MessagesTable")]
public class MessageModel
{
    [DynamoDBHashKey]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string ConversationId { get; set; }
    public string SenderId { get; set; }
    public string RecipientId { get; set; }
    /// <summary>
    /// O conteúdo é criptografado no cliente; aqui é apenas um blob cifrado
    /// </summary>
    public string Content { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    /// <summary>
    /// sent, delivered, read, dead
    /// </summary>
    public string Status { get; set; } = "sent";
    /// <summary>
    /// Optimistic concurrency
    /// </summary>
    public int Version { get; set; } = 0;
    public string IdempotencyKey { get; set; }
    public int DeliveryAttempts { get; set; } = 0;

    /// <summary>
    /// TTL (Unix epoch seconds) - DynamoDB TTL enabled on ExpiresAtUnix
    /// </summary>
    public long? ExpiresAtUnix { get; set; }
}