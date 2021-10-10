namespace Couchbase.Fake.Models;

public class ErrorDescription
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<ErrorAttribute>? Attrs { get; set; }
}
