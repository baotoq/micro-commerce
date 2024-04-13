namespace Infrastructure.Common.Options;

public class ElasticsearchOptions
{
    public const string Key = "Elasticsearch";

    public string Url { get; set; } = "http://localhost:9200";
}