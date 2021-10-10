using Couchbase.Fake.Types;

namespace Couchbase.Fake.Interfaces;

public interface ISaslSession
{
    bool Challenge(SaslMessage challenge, out SaslMessage response);
}
