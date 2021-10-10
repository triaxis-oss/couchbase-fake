namespace Couchbase.Fake.Interfaces;

public interface ISaslProvider
{
    ISaslSession CreateSession(string name, bool server);
}
