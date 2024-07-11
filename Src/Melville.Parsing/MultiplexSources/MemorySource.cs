namespace Melville.Parsing.MultiplexSources;

internal class MemorySource (Memory<byte> data) : IMultiplexSource, IIndexedReader
{
    public void Dispose() { }

    public Stream ReadFrom(long position) =>
        IndexedReaderStreamFactory.Shared.Rent().ReadFrom(this, position);

    public long Length => data.Length;

    int IIndexedReader.Read(long position, in Span<byte> buffer)
    {
        var length = (int)Math.Min(buffer.Length, data.Length - position);
        data.Span.Slice((int)position, length).CopyTo(buffer);
        return length;
    }

    ValueTask<int> IIndexedReader.ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken) =>
        new(((IIndexedReader)this).Read(position, buffer.Span));
}