using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.Streams;

/// <summary>
/// This is a stream that has multiple simultaneous readers.
/// </summary>
public sealed class MultiplexedStream: IDisposable
{
    private Stream source;
    /// <summary>
    /// The length of the underlying stream.
    /// </summary>
    public long Length => source.Length;
    private SemaphoreSlim mutex = new SemaphoreSlim(1);

    /// <summary>
    /// Create a MultiplexedStream from a stream </summary>
    /// <param name="source">The underlysing stream, which must be seekable and readable.</param>
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

    /// <summary>
    /// Read bytes into a span.
    /// </summary>
    /// <param name="position">The position to read from</param>
    /// <param name="buffer">A span to hold the read data.</param>
    /// <returns>The number of bytes read</returns>
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

    /// <summary>
    /// Asynchronously read from the stream to a memory
    /// </summary>
    /// <param name="position">Position to start reading from.</param>
    /// <param name="buffer">A memory to hold the read data</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns></returns>
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

    /// <summary>
    /// Create a multiplexed reader that begins at a given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Stream ReadFrom(long position) => new MultiplexedReader(this, position);
}

internal sealed class MultiplexedReader : DefaultBaseStream
{
    private readonly MultiplexedStream source;

    public MultiplexedReader(MultiplexedStream source, long position):
        base(true, false, true)
    {
        this.source = source;
        Position = position;
    }
    public override int Read(Span<byte> buffer)
    {
        var ret = source.Read(Position, buffer);
        Position += ret;
        return ret;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        var ret = await source.ReadAsync(Position, buffer, cancellationToken).CA();
        Position += ret;
        return ret;
    }
    
    public override long Seek(long offset, SeekOrigin origin) =>
        Position = SeekTargetFromStreamStart(offset, origin);

    private long SeekTargetFromStreamStart(long offset, SeekOrigin origin) =>
        origin switch {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => source.Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };
    
    public override long Length => source.Length;
}