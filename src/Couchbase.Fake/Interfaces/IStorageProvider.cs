using System;

namespace Couchbase.Fake.Interfaces
{
    public interface IStorageProvider
    {
        Task<IStorageBucket> GetBucketAsync(string bucketName);
    }
}
