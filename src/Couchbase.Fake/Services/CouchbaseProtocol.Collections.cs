using Couchbase.Fake.Models;

namespace Couchbase.Fake.Services;

partial class CouchbaseProtocol
{
    private static readonly Manifest s_collectionManifest = new()
    {
        Uid = "42",
        Scopes = new[]
        {
            new Scope
            {
                Name = "_default",
                Uid = "0",
                Collections = new[]
                {
                    new Collection
                    {
                        Name = "_default",
                        Uid = "0",
                    },
                },
            },
        },
    };

    private Manifest GetCollectionManifest() => s_collectionManifest;
}
