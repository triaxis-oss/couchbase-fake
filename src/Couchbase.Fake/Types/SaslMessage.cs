namespace Couchbase.Fake.Types;

public readonly ref struct SaslMessage
{
    private readonly ReadOnlySpan<byte> _message;

    private delegate bool Predicate(ReadOnlySpan<byte> span);

    public SaslMessage(ReadOnlySpan<byte> message)
    {
        _message = message;
    }

    public ReadOnlySpan<byte> this[char c]
        => Find(s => s.Length >= 2 && s[0] == (byte)c && s[1] == (byte)'=')[2..];

    public bool IsValid
        => !_message.IsEmpty;

    public static implicit operator ReadOnlySpan<byte>(SaslMessage message)
        => message._message;

    private ReadOnlySpan<byte> Find(Predicate predicate)
    {
        ReadOnlySpan<byte> rest = _message;

        while (rest.Length > 2)
        {
            var split = rest.IndexOf((byte)',');
            var part = split == -1 ? rest : rest[..split];
            if (predicate(part))
            {
                return part;
            }
            if (split == -1)
            {
                break;
            }
            else
            {
                rest = rest[(split+1)..];
            }
        }

        return default;
    }
}
