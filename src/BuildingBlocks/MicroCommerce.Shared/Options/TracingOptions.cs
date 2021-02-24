namespace MicroCommerce.Shared.Options
{
    public class TracingOptions
    {
        public string ServiceName { get; set; }
        public string Endpoint { get; set; } = "http://zipkin:9411/api/v2/spans";
    }
}
