using System;

namespace Couchbase.Fake.Models
{
    public class VBucketServerMapDto
    {
        public string? HashAlgorithm { get; set; }
        public int NumReplicas { get; set; }
        public IList<string>? ServerList { get; set; }
        public short[][]? VBucketMap { get; set; }
    }
}
