using System;

namespace Couchbase.Fake.Models
{
    public class Node
    {
        public string? CouchApiBase { get; set; }
        public string? Hostname { get; set; }
        public Dictionary<string, ushort>? Ports { get; set; }
        public IList<string>? Services { get; set; }
        public string? Version { get; set; }
    }
}
