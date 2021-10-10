namespace Couchbase.Fake.Models;

[Flags]
public enum ErrorAttribute
{
    Success = 1,
    ItemDeleted = 2,
    ItemLocked = 4,
    ItemOnly = 8,
    InvalidInput = 0x10,
    FetchConfig = 0x20,
    ConnStateInvalidated = 0x40,
    Auth = 0x80,
    SpecialHandling = 0x100,
    Support = 0x200,
    Temp = 0x400,
    Internal = 0x800,
    RetryNow = 0x1000,
    RetryLater = 0x2000,
    SubDoc = 0x4000,
    Dcp = 0x8000,
    RateLimit = 0x10000,
}
