namespace Couchbase.Fake.Models;

public class ErrorMap
{
    public int Version { get; set; }
    public int Revision { get; set; }
    public IDictionary<string, ErrorDescription>? Errors { get; set; }
}
