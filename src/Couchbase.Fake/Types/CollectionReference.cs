namespace Couchbase.Fake.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct CollectionReference
{
    public long ManifestId { get; set; }
    public int CollectionId { get; set; }
}
