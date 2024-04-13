namespace Infrastructure.Common.Options;

public class RedisOptions
{
    public const string Key = "Redis";
    
    public string Host { get; set; } = "localhost";
    public string Port { get; set; } = "6371";
    
    public string ConnectionString => $"{Host}:{Port}";
}