using System;
namespace Couchbase.Fake
{
    static class StreamExtensions
    {
        public static async Task<bool> ReadExactAsync(this Stream stream, Memory<byte> memory, CancellationToken cancellationToken = default)
        {
            bool first = true;

            while (memory.Length > 0)
            {
                int block = await stream.ReadAsync(memory, cancellationToken);
                if (block == 0)
                {
                    if (first)
                    {
                        return false;
                    }
                    else
                    {
                        throw new IOException("Unexpected end of stream");
                    }
                }
                memory = memory.Slice(block);
                first = false;
            }

            return true;
        }
    }
}