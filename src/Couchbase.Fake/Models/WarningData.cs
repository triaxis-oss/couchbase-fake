namespace Couchbase.Fake.Models;

public class WarningData
{
    [JsonPropertyName("msg")]
    public string? Message { get; set; }
    [JsonPropertyName("code")]
    public int Code { get; set; }
}
