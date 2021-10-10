using System;
using System.Runtime.InteropServices;

namespace Couchbase.Fake.Interfaces
{
    public struct ValueFlags
    {
        public DataFormat DataFormat { get; set; }
        public TypeCode TypeCode { get; set; }
    }
}
