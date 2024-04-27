namespace Melville.Parsing.MultiplexSources;

internal class MemorySource (Memory<byte> data) : IMultiplexSource, IIndexedReader
{
    public void Dispose() { }

    public Stream ReadFrom(long position) => new IndexedReaderStream<MemorySource>(this, position);

    public long Length => data.Length;

    public int Read(long position, in Span<byte> buffer)
    {
        var length = (int)Math.Min(buffer.Length, data.Length - position);
        data.Span.Slice((int)position, length).CopyTo(buffer);
        return length;
    }

    public ValueTask<int> ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken) =>
        new(Read(position, buffer.Span));
}