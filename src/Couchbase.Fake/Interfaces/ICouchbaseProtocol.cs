namespace Couchbase.Fake.Interfaces;

public interface ICouchbaseProtocol
{
    Task RunAsync(Stream stream);
}
