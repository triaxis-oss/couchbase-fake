using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Couchbase.Fake.Interfaces;
using Couchbase.Fake.Types;

namespace Couchbase.Fake.Services;

partial class CouchbaseProtocol
{
    private delegate void SerializerDelegate<T>(Stream stream, T element);

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private ReadOnlyMemory<byte> TempFrom(long start)
        => new(_temp.GetBuffer(), checked((int)start), checked((int)(_temp.Position - start)));

    private ReadOnlyMemory<byte> Serialize<T>(IEnumerable<T> collection)
    {
        var start = _temp.Position;
        var serializer = Serializer<T>.Delegate;
        foreach (var us in collection)
        {
            serializer(_temp, us);
        }
        return TempFrom(start);
    }

    private ReadOnlyMemory<byte> Serialize(ReadOnlySpan<byte> span)
    {
        var start = _temp.Position;
        _temp.Write(span);
        return TempFrom(start);
    }

    private ReadOnlyMemory<byte> Serialize(ValueFlags flags)
    {
        Span<Flags> extra = stackalloc Flags[1];
        extra[0].Format = flags.DataFormat;
        extra[0].TypeCode = flags.TypeCode;
        return Serialize(MemoryMarshal.AsBytes(extra));
    }

    private static void Serialize(Stream stream, ushort element)
    {
        stream.WriteByte((byte)(element >> 8));
        stream.WriteByte((byte)element);
    }

    private ReadOnlyMemory<byte> Json<T>(T value)
    {
        var start = _temp.Position;
        JsonSerializer.Serialize(_temp, value, s_jsonOptions);
        return TempFrom(start);
    }

    private string Utf8(ReadOnlySpan<byte> value)
        => Encoding.UTF8.GetString(value);

    private string Ascii(ReadOnlySpan<byte> value)
        => Encoding.ASCII.GetString(value);

    static class Serializer<T>
    {
        public static SerializerDelegate<T> Delegate = CreateDelegate();

        private static SerializerDelegate<T> CreateDelegate()
        {
            return (SerializerDelegate<T>)System.Delegate.CreateDelegate(typeof(SerializerDelegate<T>), typeof(CouchbaseProtocol), "Serialize");
        }
    }
}
