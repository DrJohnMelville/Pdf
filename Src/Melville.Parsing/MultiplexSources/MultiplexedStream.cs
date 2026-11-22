using Melville.StreamInterfaces;

namespace Melville.Parsing.MultiplexSources;

internal sealed class MultiplexedStream : IndexedMultiplexSource
{
    private Stream source;

    public override long Length => source.Length;
    private SemaphoreSlim mutex = new SemaphoreSlim(1);

    public MultiplexedStream(Stream source)
    {
        this.source = source;
        VerifyLegalStream();
    }

    protected override void CleanUp()
    {
        mutex.Dispose(); 
        source.Dispose();
    }

    private void VerifyLegalStream()
    {
        if (!source.CanRead) throw new ArgumentException("Stream must be readable.");
        if (!source.CanSeek) throw new ArgumentException("Stream must be seekable.");
    }
    
    /// <inheritdoc />
    public override int Read(long position, in Span<byte> buffer)
    {
        mutex.Wait();
        try
        {
            EnsureProperReadPosition(position);
            return source.Read(buffer);
        }
        finally
        {
            mutex.Release();
        }
    }

    /// <inheritdoc />
    public override async ValueTask<int> ReadAsync(
        long position, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        await mutex.WaitAsync().CA();
        try
        {
            EnsureProperReadPosition(position);
            return await source.ReadAsync(buffer, cancellationToken).CA();
        }
        finally
        {
            mutex.Release();
        }
    }

    private void EnsureProperReadPosition(long position)
    {
        if (source.Position != position) source.Seek(position, SeekOrigin.Begin);
    }
}