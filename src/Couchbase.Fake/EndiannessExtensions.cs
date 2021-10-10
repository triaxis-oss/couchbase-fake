using System;

namespace Couchbase.Fake
{
    static class EndiannessExtensions
    {
        private static ushort Rev16(this ushort v) => (ushort)(v << 8 | v >> 8);
        private static uint Rev32(this uint v) => (uint)(Rev16((ushort)v) << 16) | Rev16((ushort)(v >> 16));

        public static ushort FromBE(this ushort v) => Rev16(v);
        public static uint FromBE(this uint v) => Rev32(v);

        public static ushort ToBE(this ushort v) => Rev16(v);
        public static uint ToBE(this uint v) => Rev32(v);
    }
}