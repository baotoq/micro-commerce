namespace MicroCommerce.ApiService.Infrastructure.Common.Options;

public class MessageBrokerOptions
{
    public const string Key = "MessageBroker";
    
    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 5672;
    public string User { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    
    public string ConnectionString => $"amqp://{User}:{Password}@{Host}:{Port}";
}