using System;

namespace Couchbase.Fake.Models
{
    public record ClusterConfig
    {
        public ulong Rev { get; init; }
        public ulong RevEpoch { get; init; }
        public string? Name { get; init; }
        public string? Uri { get; init; }
        public string? StreamingUri { get; init; }
        public IList<Node>? Nodes { get; init; }
        public IList<NodesExt>? NodesExt { get; init; }
        public string? NodeLocator { get; init; }
        public string? Uuid { get; init; }
        public Ddocs? Ddocs { get; init; }
        public VBucketServerMapDto? VBucketServerMap { get; init; }
        public string? BucketCapabilitiesVer { get; init; }
        public IList<string>? BucketCapabilities { get; init; }
        public IList<int>? ClusterCapabilitiesVer { get; init; }
        public IDictionary<string, IEnumerable<string>>? ClusterCapabilities { get; init; }
        public string? CollectionsManifestUid { get; set; }
    }
}
