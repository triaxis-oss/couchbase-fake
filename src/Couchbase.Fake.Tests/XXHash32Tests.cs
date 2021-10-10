using System.Text;
using Couchbase.Fake.Utility;
using Xunit;

namespace Couchbase.Fake.Tests
{
    public class XXHash32Tests
    {
        private static int XXH32(string text)
            => (int)XXHash32.Calculate(Encoding.ASCII.GetBytes(text));

        [Fact]
        public void TestWellKnownHashes()
        {
            Assert.Equal(0x3e2023cf, XXH32("test"));
            Assert.Equal(0x58abbfcb, XXH32("this is a slightly longer test"));
        }
    }
}