namespace Couchbase.Fake.Models;

public class MetricsData
{
    public string? ElapsedTime { get; set; }
    public string? ExecutionTime { get; set; }
    public uint ResultCount { get; set; }
    public uint ResultSize { get; set; }
    public uint MutationCount { get; set; }
    public uint ErrorCount { get; set; }
    public uint WarningCount { get; set; }
    public uint SortCount { get; set; }
}
