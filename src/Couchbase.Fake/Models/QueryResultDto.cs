namespace Couchbase.Fake.Models;

public class QueryResultDto<T>
{
    [JsonPropertyName("requestID")]
    public Guid? RequestId { get; set; }
    [JsonPropertyName("clientContextID")]
    public string? ClientContextId { get; set; }
    [JsonPropertyName("signature")]
    public dynamic? Signature { get; set; }
    [JsonPropertyName("results")]
    public IEnumerable<T>? Results { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("errors")]
    public IEnumerable<ErrorData>? Errors { get; set; }
    [JsonPropertyName("warnings")]
    public IEnumerable<WarningData>? Warnings { get; set; }
    [JsonPropertyName("metrics")]
    public MetricsData? Metrics { get; set; }
    [JsonPropertyName("profile")]
    public dynamic? Profile { get; set; }
    [JsonPropertyName("prepared")]
    public string? Prepared { get; set; }
}
