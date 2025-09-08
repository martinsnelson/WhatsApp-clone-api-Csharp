namespace LambdaApi.Contexts;

public interface IDynamoDBContext
{
    Task SaveAsync<T>(T entity);
    Task<T> LoadAsync<T>(object hashKey);
    Task DeleteAsync<T>(T entity);
}