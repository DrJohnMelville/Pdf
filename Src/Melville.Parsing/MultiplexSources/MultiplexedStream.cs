using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.MultiplexSources;

/// <summary>
/// This is a stream that has multiple simultaneous readers.
/// </summary>
internal sealed class MultiplexedStream : IMultiplexSource, IIndexedReader
{
    private Stream source;

    /// <inheritdoc />
    public long Length => source.Length;
    private SemaphoreSlim mutex = new SemaphoreSlim(1);

    /// <summary>
    /// Create a MultiplexedStream from a stream </summary>
    /// <param name="source">The underlying stream, which must be seekable and readable.</param>
    public MultiplexedStream(Stream source)
    {
        this.source = source;
        VerifyLegalStream();
    }

    /// <inheritdoc />
    public void Dispose()
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
    public int Read(long position, in Span<byte> buffer)
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
    public async ValueTask<int> ReadAsync(
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

    /// <inheritdoc />
    public Stream ReadFrom(long position) => new IndexedReaderStream<MultiplexedStream>(this, position);
}