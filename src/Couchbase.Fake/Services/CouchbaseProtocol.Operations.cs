using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Couchbase.Fake.Interfaces;
using Couchbase.Fake.Types;

namespace Couchbase.Fake.Services
{
    partial class CouchbaseProtocol
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        private class HandlerAttribute : Attribute
        {
            public HandlerAttribute(Opcode opcode)
            {
                Opcode = opcode;
            }

            public Opcode Opcode { get; }
        }

        private static readonly Func<CouchbaseProtocol, Task>[] s_handlers = CreateHandlerMap();
        private static byte[] s_deadbeef = { 0xde, 0xad, 0xbe, 0xef };

        private static Func<CouchbaseProtocol, Task>[] CreateHandlerMap()
        {
            var res = new Func<CouchbaseProtocol, Task>[256];

            foreach (var mi in typeof(CouchbaseProtocol).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var attr in mi.GetCustomAttributes<HandlerAttribute>())
                {
                    res[(byte)attr.Opcode] = (Func<CouchbaseProtocol, Task>)Delegate.CreateDelegate(typeof(Func<CouchbaseProtocol, Task>), null, mi);
                }
            }

            return res;
        }

        [Handler(Opcode.Noop)]
        private Task HandleNoop()
            => ReplyAsync(Status.NoError);

        [Handler(Opcode.Hello)]
        private Task HandleHello()
        {
            _hello = Utf8(Key);
            _features.Clear();
            foreach (var feature in MemoryMarshal.Cast<byte, ushort>(Value))
            {
                _features.Add((Feature)feature.ToBE());
            }
            var requestedFeatures = _features.ToArray();
            _features.IntersectWith(_options.SupportedFeatures ?? CouchbaseProtocolOptions.DefaultSupportedFeatures);
            _logger.LogInformation("Hello: {Key}, requested features: {@RequestedFeatures}, confirmed features: {@Features}", _hello, requestedFeatures, _features.ToArray());
            return ReplyAsync(Status.NoError, value: Serialize(_features));
        }

        [Handler(Opcode.SaslAuth)]
        [Handler(Opcode.SaslStep)]
        private Task HandleSasl()
        {
            _logger.LogInformation("{OpCode}: type {SaslType}, data {SaslData}", _hdr.Opcode, Utf8(Key), Utf8(Value));
            if (_hdr.Opcode == Opcode.SaslAuth)
            {
                _sasl = _saslProvider.CreateSession(Utf8(Key), true);
            }
            if (_sasl.Challenge(new Types.SaslMessage(Value), out var response))
            {
                return ReplyAsync(Status.NoError, value: Serialize(response));
            }
            else if (response.IsValid)
            {
                return ReplyAsync(Status.AuthContinue, value: Serialize(response));
            }
            else
            {
                return ReplyAsync(Status.AuthError);
            }
        }

        [Handler(Opcode.GetErrorMap)]
        private Task HandleGetErrorMap()
            => ReplyAsync(Status.NoError, value: Json(s_errorMap));

        [Handler(Opcode.SelectBucket)]
        private async Task HandleSelectBucket()
        {
            _bucket = Utf8(Key);
            _bucketUuid = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Key)[..16]).ToLower();
            _logger.LogInformation("Select bucket: {BucketName} (UUID: {BucketUuid})", _bucket, _bucketUuid);
            _bucketStorage = await _storage.GetBucketAsync(_bucket);
            await ReplyAsync(Status.NoError);
        }

        [Handler(Opcode.GetClusterConfig)]
        private Task HandleGetClusterConfig()
            => ReplyAsync(Status.NoError, value: Json(GetClusterConfig()));

        [Handler(Opcode.GetCollectionManifest)]
        private Task HandleGetCollectionManifest()
            => ReplyAsync(Status.NoError, value: Json(GetCollectionManifest()));

        [Handler(Opcode.GetCollectionId)]
        private Task HandleGetCollectionId()
        {
            Span<CollectionReference> extra = stackalloc CollectionReference[1];
            var collection = Utf8(Value);
            extra[0].CollectionId = collection.GetHashCode();
            _logger.LogInformation("Get CID: {Collection} => {CID}", collection, (uint)extra[0].CollectionId);
            return ReplyAsync(Status.NoError, extras: Serialize(MemoryMarshal.AsBytes(extra)));
        }

        [Handler(Opcode.Get)]
        [Handler(Opcode.GetQ)]
        [Handler(Opcode.GetK)]
        [Handler(Opcode.GetKQ)]
        private async Task HandleGet()
        {
            if (_bucketStorage == null)
            {
                await ReplyAsync(Status.NoBucket);
                return;
            }

            bool quiet = _hdr.Opcode == Opcode.GetQ || _hdr.Opcode == Opcode.GetKQ;
            bool returnKey = _hdr.Opcode == Opcode.GetK || _hdr.Opcode == Opcode.GetKQ;

            var (flags, value) = await _bucketStorage.GetAsync(KeyMemory);
            if (value == null)
            {
                _logger.LogInformation("Get {Key}: not found", Ascii(Key));
                if (quiet)
                {
                    // no reply
                    return;
                }
            }
            else
            {
                _logger.LogInformation("Get {Key}: {Value}", Ascii(Key), Ascii(Value));
            }

            await ReplyAsync(value == null ? Status.KeyNotFound : Status.NoError,
                extras: Serialize(flags),
                key: returnKey ? KeyMemory : null,
                value: value);
        }

        [Handler(Opcode.Set)]
        [Handler(Opcode.SetQ)]
        [Handler(Opcode.Add)]
        [Handler(Opcode.AddQ)]
        [Handler(Opcode.Replace)]
        [Handler(Opcode.ReplaceQ)]
        private async Task HandleSet()
        {
            if (_bucketStorage == null)
            {
                await ReplyAsync(Status.NoBucket);
                return;
            }

            bool quiet = _hdr.Opcode == Opcode.SetQ || _hdr.Opcode == Opcode.AddQ || _hdr.Opcode == Opcode.ReplaceQ;

            var mode = (_hdr.Opcode == Opcode.Add || _hdr.Opcode == Opcode.AddQ) ? SetMode.Add :
                (_hdr.Opcode == Opcode.Replace || _hdr.Opcode == Opcode.ReplaceQ) ? SetMode.Replace :
                SetMode.Set;

            var flags = MemoryMarshal.Read<Flags>(Extras);

            switch (await _bucketStorage.SetAsync(KeyMemory, flags, ValueMemory, mode))
            {
                case SetResult.Success:
                    _logger.LogInformation("{OpCode} {Key}: {Value}", _hdr.Opcode, Ascii(Key), Ascii(Value));
                    if (!quiet)
                    {
                        await ReplyAsync(Status.NoError);
                    }
                    break;

                case SetResult.KeyExists:
                    _logger.LogInformation("Failed to {OpCode} {Key}: already exists", _hdr.Opcode, Ascii(Key));
                    await ReplyAsync(Status.KeyExists);
                    break;

                case SetResult.KeyNotFound:
                    _logger.LogInformation("Failed to {OpCode} {Key}: not found", _hdr.Opcode, Ascii(Key));
                    await ReplyAsync(Status.KeyNotFound);
                    break;

                default:
                    await ReplyAsync(Status.UnknownCommand);
                    break;
            }
        }
    }
}
