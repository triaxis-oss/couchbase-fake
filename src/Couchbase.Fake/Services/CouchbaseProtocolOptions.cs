namespace Couchbase.Fake.Services;

class CouchbaseProtocolOptions
{
    public static readonly ICollection<CouchbaseProtocol.Feature> DefaultSupportedFeatures = new[]
    {
        CouchbaseProtocol.Feature.AltRequest,
        CouchbaseProtocol.Feature.SelectBucket,
        CouchbaseProtocol.Feature.Collections,
    };

    public int MaximumPacketLength { get; set; } = 1024 * 1024;
    public ICollection<CouchbaseProtocol.Feature>? SupportedFeatures { get; set; }
}
