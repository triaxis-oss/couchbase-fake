using System;
using System.Runtime.InteropServices;

namespace Couchbase.Fake.Utility
{
    /// <summary>
    /// xxHash32 implemented according to https://fossies.org/linux/xxHash/doc/xxhash_spec.md
    /// </summary>
    public static class XXHash32
    {
        private const uint PRIME32_1 = 0x9E3779B1U;  // 0b10011110001101110111100110110001
        private const uint PRIME32_2 = 0x85EBCA77U;  // 0b10000101111010111100101001110111
        private const uint PRIME32_3 = 0xC2B2AE3DU;  // 0b11000010101100101010111000111101
        private const uint PRIME32_4 = 0x27D4EB2FU;  // 0b00100111110101001110101100101111
        private const uint PRIME32_5 = 0x165667B1U;

        struct UInt32x4
        {
            public uint a, b, c, d;
        }

        public static uint Calculate(ReadOnlySpan<byte> data, uint seed = 0)
        {
            var data128 = MemoryMarshal.Cast<byte, UInt32x4>(data);
            var data32 = MemoryMarshal.Cast<byte, uint>(data[(data.Length & ~15)..]);
            var data8 = data[(data.Length & ~3)..];

            uint acc;

            if (data128.Length > 0)
            {
                // Step 1. Initialize internal accumulators
                UInt32x4 accx = new()
                {
                    a = seed + PRIME32_1 + PRIME32_2,
                    b = seed + PRIME32_2,
                    c = seed + 0,
                    d = seed - PRIME32_1,
                };

                // Step 2. Process stripes
                foreach (var input in data128)
                {
                    accx.a = Round(accx.a, input.a);
                    accx.b = Round(accx.b, input.b);
                    accx.c = Round(accx.c, input.c);
                    accx.d = Round(accx.d, input.d);
                }

                // Step 3. Accumulator convergence
                acc = Rol(accx.a, 1) + Rol(accx.b, 7) + Rol(accx.c, 12) + Rol(accx.d, 18);
            }
            else
            {
                // short input, skip to step 4
                acc = seed + PRIME32_5;
            }

            // Step 4. Add input length
            acc += (uint)data.Length;

            // Step 5. Consume remaining input
            foreach (uint input in data32)
            {
                acc = Rol(acc + input * PRIME32_3, 17) * PRIME32_4;
            }

            foreach (byte input in data8)
            {
                acc = Rol(acc + input * PRIME32_5, 11) * PRIME32_1;
            }

            // Step 6. Final mix (avalanche)
            acc ^= acc >> 15;
            acc *= PRIME32_2;
            acc ^= acc >> 13;
            acc *= PRIME32_3;
            acc ^= acc >> 16;

            return acc;
        }

        private static uint Rol(uint value, int by)
            => (value << by) | (value >> (32 - by));

        private static uint Round(uint acc, uint lane)
            => Rol(acc + lane * PRIME32_2, 13) * PRIME32_1;
    }
}
