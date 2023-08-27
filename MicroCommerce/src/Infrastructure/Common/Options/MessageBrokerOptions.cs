namespace Infrastructure.Common.Options;

public class MessageBrokerOptions
{
    public const string Key = "MessageBroker";
    
    public string Host { get; set; } = string.Empty;
    public ushort Port { get; set; }
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}