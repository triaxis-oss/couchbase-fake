using System;

namespace Couchbase.Fake.Models
{
    public class NodesExt
    {
        public bool ThisNode { get; set; }
        public Dictionary<string, ushort>? Services { get; set; }
    }
}
