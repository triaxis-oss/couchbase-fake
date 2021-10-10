namespace Couchbase.Fake.Utility;

class ProtocolStream : Stream
{
    private readonly Stream _stream;
    private int _read, _written;

    public ProtocolStream(Stream stream)
    {
        _stream = stream;
    }

    public int BytesRead => _read;
    public int BytesWritten => _written;
    public MemoryStream? CaptureStream { get; set; }

    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override bool CanRead => _stream.CanRead;

    public override int Read(byte[] buffer, int offset, int count)
    {
        int n = _stream.Read(buffer, offset, count);
        _read += n;
        return n;
    }

    public override int Read(Span<byte> buffer)
    {
        int n = _stream.Read(buffer);
        _read += n;
        return n;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _stream.ReadAsync(buffer, offset, count, cancellationToken)
            .ContinueWith(t => { _read += t.Result; return t.Result; },
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var res = _stream.ReadAsync(buffer, cancellationToken);
        if (res.IsCompleted)
        {
            if (res.IsCompletedSuccessfully)
            {
                _read += res.Result;
            }
            return res;
        }

        return new ValueTask<int>(res.AsTask().ContinueWith(t => { _read += t.Result; return t.Result; },
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion));
    }

    public override int ReadByte()
    {
        int b = _stream.ReadByte();
        if (b >= 0)
        {
            _read++;
        }
        return b;
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        throw new NotSupportedException();
    }

    public override bool CanWrite => _stream.CanWrite;

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
        CaptureStream?.Write(buffer, offset, count);
        _written += count;
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _stream.Write(buffer);
        CaptureStream?.Write(buffer);
        _written += buffer.Length;
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _stream.WriteAsync(buffer, offset, count, cancellationToken)
            .ContinueWith(t => { _written += count; CaptureStream?.Write(buffer, offset, count); },
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var res = _stream.WriteAsync(buffer, cancellationToken);
        if (res.IsCompleted)
        {
            if (res.IsCompletedSuccessfully)
            {
                CaptureStream?.Write(buffer.Span);
                _written += buffer.Length;
            }
            return res;
        }

        return new ValueTask(res.AsTask().ContinueWith(t => { _written += buffer.Length; CaptureStream?.Write(buffer.Span); },
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion));
    }

    public override void WriteByte(byte value)
    {
        _stream.WriteByte(value);
        CaptureStream?.WriteByte(value);
        _written++;
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        throw new NotSupportedException();
    }

    public override void Flush() => _stream.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _stream.FlushAsync(cancellationToken);
}
