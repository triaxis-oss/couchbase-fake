using System;

namespace Couchbase.Fake.Models
{
    public class Manifest
    {
        public string? Uid { get; set; }
        public IList<Scope>? Scopes { get; set; }
    }
}
