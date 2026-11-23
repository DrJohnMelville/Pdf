namespace Melville.Parsing.MultiplexSources;

internal abstract class IndexedMultiplexSource : CountedMultiplexSource
{
    protected override Stream ReadFromOverride(long position) =>
        IndexedMultiplexSourceWithData.TryCreateFrom(this, out var withData) ?
        new IndexedReaderStreamWithBuffer(withData, position) :
    new IndexedReaderStream(this, position);

    protected override IMultiplexReader ReaderFromOverride(long position) =>
        IndexedMultiplexSourceWithData.TryCreateFrom(this, out var withData) ?
        new IIndexedReaderWithBuffer(withData, position) :
        new IndexedReader(this, position);

    /// <summary>
    /// Read bytes into a span.
    /// </summary>
    /// <param name="position">The position to read from</param>
    /// <param name="buffer">A span to hold the read data.</param>
    /// <returns>The number of bytes read</returns>
    public abstract int Read(long position, in Span<byte> buffer);

    /// <summary>
    /// Asynchronously read from the stream to a memory
    /// </summary>
    /// <param name="position">Position to start reading from.</param>
    /// <param name="buffer">A memory to hold the read data</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The number of bytes read</returns>
    public abstract ValueTask<int> ReadAsync(
        long position, Memory<byte> buffer, CancellationToken cancellationToken);
}
