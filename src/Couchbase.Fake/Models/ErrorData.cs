namespace Couchbase.Fake.Models;

public class ErrorData
{
    [JsonPropertyName("msg")]
    public string? Message { get; set; }
    [JsonPropertyName("code")]
    public int Code { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("sev")]
    public Severity Severity { get; set; }
    [JsonPropertyName("temp")]
    public bool Temporary { get; set; }
}
