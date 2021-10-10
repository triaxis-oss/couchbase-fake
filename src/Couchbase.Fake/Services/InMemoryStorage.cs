using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Couchbase.Fake.Interfaces;
using Couchbase.Fake.Utility;

namespace Couchbase.Fake.Services
{
    public class InMemoryStorage : IStorageProvider
    {
        private readonly ConcurrentDictionary<string, IStorageBucket> _buckets
            = new ConcurrentDictionary<string, IStorageBucket>();

        public Task<IStorageBucket> GetBucketAsync(string bucketName)
            => Task.FromResult(_buckets.GetOrAdd(bucketName, b => new Bucket(b)));

        private class Bucket : IStorageBucket
        {
            public Bucket(string name)
            {
                Name = name;
            }

            private ConcurrentDictionary<Key, Slot> _data = new ConcurrentDictionary<Key, Slot>();

            private struct Key : IEquatable<Key>
            {
                private readonly int _hashCode;
                private readonly ReadOnlyMemory<byte> _key;

                public Key(ReadOnlyMemory<byte> key)
                {
                    _key = key;
                    _hashCode = (int)XXHash32.Calculate(key.Span);
                }

                private Key(ReadOnlyMemory<byte> key, int hash)
                {
                    _key = key;
                    _hashCode = hash;
                }

                public bool Equals(Key other) => _hashCode == other._hashCode && _key.Span.SequenceEqual(other._key.Span);

                public override int GetHashCode() => _hashCode;

                public Key Copy() => new Key(_key.ToArray(), _hashCode);
            }

            private class Slot
            {
                private readonly Key _key;
                private Entry? _entry;

                public Slot(Key key)
                {
                    _key = key;
                }

                public Entry? Entry => _entry;

                public SetResult Update(ValueFlags flags, ReadOnlyMemory<byte> value, SetMode mode)
                {
                    switch (mode)
                    {
                        case SetMode.Add:
                            // value must not exist
                            if (_entry != null || Interlocked.CompareExchange(ref _entry, new Entry(flags, value.ToArray()), null) != null)
                            {
                                return SetResult.KeyExists;
                            }
                            return SetResult.Success;

                        case SetMode.Replace:
                            // value must exist
                            if (_entry == null)
                            {
                                return SetResult.KeyNotFound;
                            }
                            goto default;

                        default:
                            // simply replce the value
                            Interlocked.Exchange(ref _entry, new Entry(flags, value.ToArray()));
                            return SetResult.Success;
                    }
                }
            }

            private class Entry
            {
                private readonly ValueFlags _flags;
                private readonly byte[] _value;

                public Entry(ValueFlags flags, byte[] value)
                {
                    _flags = flags;
                    _value = value;
                }

                public ValueFlags Flags => _flags;
                public ReadOnlyMemory<byte> Value => _value;
            }

            public string Name { get; }

            private Slot GetSlot(ReadOnlyMemory<byte> key)
            {
                var k = new Key(key);
                return _data.TryGetValue(new Key(key), out var slot) ? slot : _data.GetOrAdd(k.Copy(), k => new Slot(k));
            }

            public ValueTask<(ValueFlags, ReadOnlyMemory<byte>?)> GetAsync(ReadOnlyMemory<byte> key)
            {
                var entry = GetSlot(key).Entry;
                return ValueTask.FromResult((entry?.Flags ?? default, entry?.Value));
            }

            public ValueTask<SetResult> SetAsync(ReadOnlyMemory<byte> key, ValueFlags flags, ReadOnlyMemory<byte> value, SetMode mode)
            {
                var slot = GetSlot(key);
                return ValueTask.FromResult(slot.Update(flags, value, mode));
            }
        }
    }
}
