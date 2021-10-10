using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Couchbase.Fake.Interfaces;
using Couchbase.Fake.Types;

namespace Couchbase.Fake.Services
{
    class SaslProvider : ISaslProvider
    {
        public ISaslSession CreateSession(string name, bool server)
            => (name, server) switch
            {
                ("PLAIN", true) => new SaslPlainServerSession(),
                ("SCRAM-SHA1", true) => new SaslScramServerSession(HashAlgorithmName.SHA1),
                _ => throw new NotSupportedException(),
            };

        class SaslPlainServerSession : ISaslSession
        {
            public bool Challenge(SaslMessage challenge, out SaslMessage response)
            {
                // TODO: validation
                response = default;
                return true;
            }
        }

        class SaslScramServerSession : ISaslSession
        {
            private const int IterationCount = 4096;
            private static readonly byte[] IterationCountSpan = System.Text.Encoding.ASCII.GetBytes(IterationCount.ToString());

            private readonly HashAlgorithmName _hash;
            private int state = 0;
            private byte[]? nonce, salt;

            public SaslScramServerSession(HashAlgorithmName hash)
            {
                _hash = hash;
            }

            public bool Challenge(SaslMessage challenge, out SaslMessage response)
            {
                switch (state++)
                {
                    case 0: return ClientFirst(challenge, out response);
                    case 1: return ClientChallenge(challenge, out response);
                    default: response = default; return false;
                }
            }

            private static readonly byte[] s_intBE1 = { 0, 0, 0, 1 };

            private byte[] Hi(ReadOnlySpan<byte> key, ReadOnlySpan<byte> salt, int i)
            {
                var hmac = IncrementalHash.CreateHMAC(_hash, key);
                hmac.AppendData(salt);
                hmac.AppendData(s_intBE1);

                var u = hmac.GetHashAndReset();
                var res = u;

                while (--i > 0)
                {
                    hmac.AppendData(u);
                    u = hmac.GetHashAndReset();
                    Xor(res, u);
                }

                return res;
            }

            private static void Xor(Span<byte> left, ReadOnlySpan<byte> right)
            {
                var left64 = MemoryMarshal.Cast<byte, ulong>(left);
                var right64 = MemoryMarshal.Cast<byte, ulong>(right);

                for (int i = 0; i < left64.Length; i++)
                    left64[i] ^= right64[i];
                for (int i = left64.Length * sizeof(ulong); i < left.Length; i++)
                    left[i] ^= right[i];
            }

            private bool ClientFirst(SaslMessage challenge, out SaslMessage response)
            {
                var responseBuilder = new SaslMessageBuilder();

                var cnonce = challenge['r'];
                var snonce = GenerateNonce(20);
                nonce = new byte[cnonce.Length + snonce.Length];
                salt = GenerateNonce(20);   // we can use random salt, since we have password clear text
                cnonce.CopyTo(nonce);
                snonce.CopyTo(nonce.AsSpan(cnonce.Length));
                responseBuilder.Add('r', nonce);
                responseBuilder.Add('s', salt);
                responseBuilder.Add('i', IterationCountSpan);

                response = responseBuilder;
                return false;
            }

            private bool ClientChallenge(SaslMessage challenge, out SaslMessage response)
            {
                var cnonce = challenge['r'];
                if (!cnonce.SequenceEqual(nonce))
                {
                    response = default;
                    return false;
                }
                var cproof = challenge['p'];
                // TODO: validate client proof

                var responseBuilder = new SaslMessageBuilder();
                // TODO: generate server signature
                responseBuilder.Add('v', GenerateNonce(20));
                response = responseBuilder;
                return true;
            }

            private static byte GetB64(int input)
                => input switch
                {
                    < 26 => (byte)('A' + input),
                    < 52 => (byte)('a' + input - 26),
                    < 62 => (byte)('0' + input - 52),
                    62 => (byte)'+',
                    63 => (byte)'/',
                    _ => throw new InvalidProgramException()
                };

            private static byte[] GenerateNonce(int length)
            {
                var res = RandomNumberGenerator.GetBytes(length);
                for (int i = 0; i < res.Length; i++)
                {
                    res[i] = GetB64(res[i] & 63);
                }
                return res;
            }
        }
    }
}
