namespace Couchbase.Fake.Types;

public struct SaslMessageBuilder
{
    private byte[] _data;
    private int _length;

    private void Alloc(int length)
    {
        if (_length > 0)
        {
            _length++;
        }
        int required = _length + length;
        int alloc = _data?.Length ?? 16;
        while (alloc < required)
        {
            alloc *= 2;
        }
        if (alloc != _data?.Length)
        {
            Array.Resize(ref _data, alloc);
        }
        if (_length > 0)
        {
            _data[_length - 1] = (byte)',';
        }
    }

    public void Add(ReadOnlySpan<byte> value)
    {
        Alloc(value.Length);
        value.CopyTo(_data.AsSpan(_length));
        _length += value.Length;
    }

    public void Add(char key, ReadOnlySpan<byte> value)
    {
        Alloc(2 + value.Length);
        _data[_length++] = (byte)key;
        _data[_length++] = (byte)'=';
        value.CopyTo(_data.AsSpan(_length));
        _length += value.Length;
    }

    public static implicit operator ReadOnlySpan<byte>(SaslMessageBuilder builder)
        => builder.ToSpan();

    public static implicit operator SaslMessage(SaslMessageBuilder builder)
        => builder.Build();

    public ReadOnlySpan<byte> ToSpan()
        => _length > 0 ? new ReadOnlySpan<byte>(_data, 0, _length) : default;

    public SaslMessage Build()
        => new(ToSpan());
}
