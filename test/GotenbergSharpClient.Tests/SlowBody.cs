namespace GotenbergSharpClient.Tests;

/// <summary>
/// A body of <paramref name="chunks" /> bytes that waits <paramref name="pause" /> before each byte but the first:
/// with an infinite pause, it stalls after the first byte. Like a network stream, a read that waits ends when its
/// token fires or when the stream is disposed.
/// </summary>
internal sealed class SlowBody(int chunks, TimeSpan pause) : Stream
{
    private readonly CancellationTokenSource _disposed = new();
    private int _sent;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(this._disposed.IsCancellationRequested, this);

        if (this._sent == chunks || buffer.IsEmpty) return 0;

        if (this._sent > 0)
        {
            using var wait = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this._disposed.Token);

            try
            {
                await Task.Delay(pause, wait.Token);
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException("The read was canceled.", null, cancellationToken);

                throw new ObjectDisposedException(nameof(SlowBody));
            }
        }

        buffer.Span[0] = (byte)'%';
        this._sent++;

        return 1;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing) this._disposed.Cancel();

        base.Dispose(disposing);
    }
}
