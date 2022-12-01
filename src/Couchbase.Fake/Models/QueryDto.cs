namespace Couchbase.Fake.Models;

public class QueryDto
{
    [JsonPropertyName("statement")]
    public string? Statement { get; set; }
    [JsonPropertyName("timeout")]
    public string? Timeout { get; set; }
    [JsonPropertyName("client_context_id")]
    public string? ClientContextId { get; set; }
}
