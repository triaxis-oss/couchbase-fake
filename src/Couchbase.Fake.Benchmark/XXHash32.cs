using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace Couchbase.Fake.Benchmark
{
    [DisassemblyDiagnoser(printSource: true)]
    public class XXHash32
    {
        private readonly byte[] random = new byte[1024*1024];

        public XXHash32()
        {
            new Random().NextBytes(random);
        }

        [Benchmark]
        public void Calculate1M()
        {
            Utility.XXHash32.Calculate(random);
        }
    }
}
