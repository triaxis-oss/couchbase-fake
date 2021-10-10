using System;

namespace Couchbase.Fake.Interfaces
{
    public enum SetMode
    {
        Set, Add, Replace,
    }

    public enum SetResult
    {
        Success, KeyNotFound, KeyExists,
    }

    public interface IStorageBucket
    {
        ValueTask<(ValueFlags, ReadOnlyMemory<byte>?)> GetAsync(ReadOnlyMemory<byte> key);
        ValueTask<SetResult> SetAsync(ReadOnlyMemory<byte> key, ValueFlags flags, ReadOnlyMemory<byte> value, SetMode mode);
    }
}
