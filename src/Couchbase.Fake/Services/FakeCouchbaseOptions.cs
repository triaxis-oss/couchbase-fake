using System.Net;

namespace Couchbase.Fake.Services;

class FakeCouchbaseOptions
{
    public string ListenHost { get; set; } = "127.0.0.1";
    public int ListenPort { get; set; } = 11210;
}
