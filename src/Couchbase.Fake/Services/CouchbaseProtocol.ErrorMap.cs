using Couchbase.Fake.Models;

namespace Couchbase.Fake.Services;

partial class CouchbaseProtocol
{
    private static readonly ErrorMap s_errorMap = new()
    {
        Version = 1,
        Revision = 1,
        Errors = CollectErrors(),
    };

    private static IDictionary<string, ErrorDescription> CollectErrors()
    {
        var map = new Dictionary<string, ErrorDescription>();

        foreach (var t in typeof(ErrorMap).Assembly.GetTypes())
        {

        }

        return map;
    }
}
