using System.Runtime.InteropServices;
using Couchbase.Fake.Interfaces;

namespace Couchbase.Fake.Services;

partial class CouchbaseProtocol
{
    enum Magic : byte
    {
        Request = 0x80,
        Response = 0x81,
        ServerRequest = 0x82,
        ServerResponse = 0x83,
    }

    enum Opcode : byte
    {
        Get = 0x00, GetQ = 0x09,
        GetK = 0x0c, GetKQ = 0x0d,
        Set = 0x01, SetQ = 0x11,
        Add = 0x02, AddQ = 0x12,
        Replace = 0x03, ReplaceQ = 0x13,
        Delete = 0x04, DeleteQ = 0x14,
        Increment = 0x05, IncrementQ = 0x15,
        Decrement = 0x06, DecrementQ = 0x16,
        Quit = 0x07, QuitQ = 0x17,
        Flush = 0x08, FlushQ = 0x18,
        Noop = 0x0a,
        Hello = 0x1f,
        SaslAuth = 0x21,
        SaslStep = 0x22,
        SelectBucket = 0x89,
        GetClusterConfig = 0xb5,
        GetCollectionManifest = 0xba,
        GetErrorMap = 0xfe,
    }

    enum DataType : byte
    {

    }

    enum Status : ushort
    {
        NoError = 0,
        KeyNotFound = 1,
        KeyExists = 2,
        NoBucket = 8,
        AuthError = 0x20,
        AuthContinue = 0x21,
        UnknownCommand = 0x81,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Header
    {
        private Magic _magic;
        private Opcode _opCode;
        private ushort _keyLength;
        private byte _extrasLength;
        private DataType _dataType;
        private ushort _bucketOrStatus;
        private uint _length;
        private uint _opaque;
        private ulong _cas;

        public Magic Magic => _magic;
        public Opcode Opcode => _opCode;
        public ushort KeyLength
        {
            get => _keyLength.FromBE();
            set => _keyLength = value.ToBE();
        }
        public byte ExtrasLength
        {
            get => _extrasLength;
            set => _extrasLength = value;
        }
        public DataType DataType => _dataType;
        public ushort Bucket => _bucketOrStatus.FromBE();
        public Status Status
        {
            get => (Status)_bucketOrStatus.FromBE();
            set => _bucketOrStatus = ((ushort)value).ToBE();
        }
        public uint Length => _length.FromBE();
        public uint Opaque => _opaque.FromBE();
        public ulong CAS => _cas;
        public uint ValueLength
        {
            get => Length - KeyLength - ExtrasLength;
            set => _length = checked(value + KeyLength + ExtrasLength).ToBE();
        }

        public void ToggleDirection()
        {
            _magic = (Magic)((byte)_magic ^ 1);
        }

        public byte[] ToByteArray()
            => MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref this, 1)).ToArray();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Flags
    {
        private DataFormat _format;
        private byte _reserved;
        private ushort _typeCode;

        public DataFormat Format
        {
            get => _format;
            set => _format = value;
        }
        public Interfaces.TypeCode TypeCode
        {
            get => (Interfaces.TypeCode)_typeCode.FromBE();
            set => _typeCode = ((ushort)value).ToBE();
        }
        
        public static implicit operator ValueFlags(Flags flags)
            => new ValueFlags { DataFormat = flags.Format, TypeCode = flags.TypeCode };
    }

    public enum Feature : ushort
    {
        DataType = 1,
        Tls = 2,
        TcpNoDelay = 3,
        MutationSeqNo = 4,
        TcpDelay = 5,
        XAttr = 6,
        XError = 7,
        SelectBucket = 8,
        Snappy = 0xa,
        Json = 0xb,
        Duplex = 0xc,
        ClustermapChange = 0xd,
        Unordered = 0xe,
        Tracing = 0xf,
        AltRequest = 0x10,
        SyncReplication = 0x11,
        Collections = 0x12,
        OpenTelemetry = 0x13,
        PreserveTtl = 0x14,
        VAttr = 0x15,
        PiTR = 0x16,
        SubdocCreateAsDeleted = 0x17,
        SubdocDocumentMacroSupport = 0x18,
        SubdocReplaceBodyWithXattr = 0x19,
    }
}
