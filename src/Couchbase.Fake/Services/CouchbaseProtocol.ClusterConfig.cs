using Couchbase.Fake.Models;

namespace Couchbase.Fake.Services;

partial class CouchbaseProtocol
{
    private static readonly ClusterConfig s_clusterConfig = new()
    {
        Rev = 42, RevEpoch = 1,
        NodesExt = new[]
        {
            new NodesExt { ThisNode = true, Services = new() { ["kv"] = 11210, ["mgmt"] = 8091 } },
        },
    };

    private const int VBucketCount = 64;

    private ClusterConfig GetClusterConfig()
    {
        var cc = s_clusterConfig;

        if (_bucket != null)
        {
            cc = cc with
            {
                Name = _bucket,
                NodeLocator = "vbucket",
                Uuid = _bucketUuid,
                Uri = $"/pools/default/buckets/{_bucket}?bucket_uuid={_bucketUuid}",
                //StreamingUri = $"/pools/default/bucketsStreaming/{_bucket}?bucket_uuid={_bucketUuid}",
                CollectionsManifestUid = "0",
                BucketCapabilitiesVer = "",
                BucketCapabilities = new[]
                {
                    "collections",
                },
                Nodes = new[]
                {
                    new Node
                    {
                        CouchApiBase = $"http://$HOST:8091/{_bucket}%2B{_bucketUuid}",
                        Hostname = "$HOST:8091",
                        Ports = new()
                        {
                            ["direct"] = 11210,
                        },
                    },
                },
                VBucketServerMap = new()
                {
                    HashAlgorithm = "CRC",
                    NumReplicas = 0,
                    ServerList = new[]
                    {
                        "$HOST:11210",
                    },
                    VBucketMap = Enumerable.Range(0, VBucketCount).Select(_ => new short[] { 0 }).ToArray(),
                },
            };
        }

        return cc;
    }
}
