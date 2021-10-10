using System;

namespace Couchbase.Fake.Models
{
    public class Scope
    {
        public string? Name { get; set; }
        public string? Uid { get; set; }
        public IList<Collection>? Collections { get; set; }
    }
}
