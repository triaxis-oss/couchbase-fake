using System;
using System.Runtime.InteropServices;
using System.Text;
using Couchbase.Fake.Interfaces;
using Couchbase.Fake.Utility;
using Microsoft.Extensions.Options;

namespace Couchbase.Fake.Services
{
    partial class CouchbaseProtocol : ICouchbaseProtocol
    {
        private readonly ISaslProvider _saslProvider;
        private readonly IStorageProvider _storage;
        private readonly CouchbaseProtocolOptions _options;
        private readonly ILogger _logger;

        private Stream _stream = null!;
        private readonly byte[] _headerBuf = new byte[Marshal.SizeOf<Header>()];
        private Header _hdr;
        private byte[]? _payload;
        private readonly MemoryStream _temp = new();

        private string _hello = null!;
        private HashSet<Feature> _features = new();
        private ISaslSession _sasl = null!;
        private string? _bucket, _bucketUuid;
        private IStorageBucket? _bucketStorage;

        private Span<byte> Extras => _payload.AsSpan(0, _hdr.ExtrasLength);
        private Span<byte> Key => _payload.AsSpan(_hdr.ExtrasLength, _hdr.KeyLength);
        private Span<byte> Value => _payload.AsSpan(_hdr.ExtrasLength + _hdr.KeyLength);
        private Memory<byte> ExtrasMemory => _payload.AsMemory(0, _hdr.ExtrasLength);
        private Memory<byte> KeyMemory => _payload.AsMemory(_hdr.ExtrasLength, _hdr.KeyLength);
        private Memory<byte> ValueMemory => _payload.AsMemory(_hdr.ExtrasLength + _hdr.KeyLength);

        public CouchbaseProtocol(
            ISaslProvider saslProvider,
            IStorageProvider storage,
            IOptions<CouchbaseProtocolOptions> options,
            ILogger<CouchbaseProtocol> logger)
        {
            _saslProvider = saslProvider;
            _storage = storage;
            _options = options.Value;
            _logger = logger;
        }

        public async Task RunAsync(Stream stream)
        {
            _stream = stream;

            while (await stream.ReadExactAsync(_headerBuf))
            {
                _hdr = MemoryMarshal.Read<Header>(_headerBuf);
                if (_hdr.Length > _options.MaximumPacketLength)
                {
                    _logger.LogError("!! Packet length {PacketLength} above maximum {MaximumPacketLength}", _hdr.Length, _options.MaximumPacketLength);
                    return;
                }
                _payload = new byte[_hdr.Length];
                await stream.ReadExactAsync(_payload);

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("<< Header: {HeaderLength} ({@Header})", _headerBuf.Length, _hdr);
                    if (_hdr.ExtrasLength > 0)
                    {
                        _logger.LogTrace("   Extras: {Extras} ({ExtrasLength})",
                            Ascii(Extras), _hdr.ExtrasLength);
                    }
                    if (_hdr.KeyLength > 0)
                    {
                        _logger.LogTrace("   Key: {Key} ({KeyLength})",
                            Ascii(Key), _hdr.KeyLength);
                    }
                    if (_hdr.ValueLength > 0)
                    {
                        _logger.LogTrace("   Value: {Value} ({ValueLength})",
                            Ascii(Value), _hdr.ValueLength);
                    }
                }

                await HandleAsync();

                _payload = null;
                _temp.SetLength(0);
            }
        }

        private Task HandleAsync()
        {
            var handler = s_handlers[(byte)_hdr.Opcode];

            if (handler == null)
            {            
                _logger.LogError("Unknown opcode: {OpCode}", _hdr.Opcode);
                return ReplyAsync(Status.UnknownCommand);
            }
            else
            {
                return handler(this);
            }
        }

        private async Task ReplyAsync(Status status, ReadOnlyMemory<byte>? extras = null, ReadOnlyMemory<byte>? key = null, ReadOnlyMemory<byte>? value = null)
        {
            var replyHdr = _hdr;
            replyHdr.ToggleDirection();
            replyHdr.Status = status;
            replyHdr.ExtrasLength = checked((byte)(extras?.Length ?? 0));
            replyHdr.KeyLength = checked((ushort)(key?.Length ?? 0));
            replyHdr.ValueLength = checked((uint)(value?.Length ?? 0));
            await _stream.WriteAsync(replyHdr.ToByteArray());

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(">> Header: {HeaderLength} ({@Header})", _headerBuf.Length, replyHdr);
            }

            await WriteOutputAsync("Extras", extras);
            await WriteOutputAsync("Key", key);
            await WriteOutputAsync("Value", value);

            ValueTask WriteOutputAsync(string name, ReadOnlyMemory<byte>? block)
            {
                if (block is ReadOnlyMemory<byte> blk)
                {
                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace($"   {name}: {{{name}Length}} ({{{name}Hex}})",
                            blk.Length, Convert.ToHexString(blk.Span));
                    }

                    return _stream.WriteAsync(blk);
                }

                return ValueTask.CompletedTask;
            }

            await _stream.FlushAsync();
        }
    }
}
