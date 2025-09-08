using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace LambdaApi.Contexts;

public class DynamoDBContext : IDynamoDBContext
{
    private readonly IAmazonDynamoDB _IAmazonDynamoDB;
    private readonly Amazon.DynamoDBv2.DataModel.DynamoDBContext _DataModelDynamoDB;

    public DynamoDBContext(IAmazonDynamoDB amazonDynamoDB, Amazon.DynamoDBv2.DataModel.DynamoDBContext dataModelDynamoDB)
    {
        _IAmazonDynamoDB = amazonDynamoDB;
        _DataModelDynamoDB = dataModelDynamoDB;
    }

    public Task SaveAsync<T>(T entity) => _DataModelDynamoDB.SaveAsync(entity);
    public Task<T> LoadAsync<T>(object hashKey) => _DataModelDynamoDB.LoadAsync<T>(hashKey);
    public Task DeleteAsync<T>(T entity) => _DataModelDynamoDB.DeleteAsync(entity);
}