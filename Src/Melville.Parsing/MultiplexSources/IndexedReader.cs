using Melville.IntersectionTypes;
using Melville.StreamInterfaces;

namespace Melville.Parsing.MultiplexSources;

internal class IndexedReader(IndexedMultiplexSource inner, long _basePosition) :
    IMultiplexReader, ISyncReader, IHasSourceTicket, IDisposable, IMultiplexSource
{
    IndexedMultiplexSource inner = inner;
    protected readonly long basePosition = _basePosition;
    long absolutePosition = _basePosition;
    CountedSourceTicket IHasSourceTicket.SourceTicket { get; set; }

    int BumpPosition(int delta)
    {
        absolutePosition += delta;
        return delta;
    }

    public async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellation = default) =>
        BumpPosition(await inner.ReadAsync(absolutePosition, buffer, cancellation).CA());

    public int Read(Span<byte> buffer) =>
        BumpPosition(inner.Read(absolutePosition, buffer));

    void ISeekableStream.Seek(long position) => Seek(position);

    public long Seek(long position)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)position, (ulong)Length);
        absolutePosition = basePosition + position;
        ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)absolutePosition, (ulong)inner.Length);
        if ((ulong)absolutePosition > (ulong)inner.Length)
            throw new InvalidOperationException("Invalid Reader Position");
        return Position;
    }

    public long Position => absolutePosition - basePosition;

    public long Length => inner.Length - basePosition;

    public void Dispose()
    {
        if (inner is null) return;
        inner = null!;
        ((IHasSourceTicket)this).SourceTicket.Release(this);
    }

    public Stream ReadFrom(long position) => inner.ReadFrom(basePosition + position);

    public IMultiplexReader ReaderFrom(long position) => inner.ReaderFrom(basePosition + position);

    public IMultiplexSource OffsetFrom(uint offset) => inner.OffsetFrom((uint)(basePosition + Position));
}


internal class IIndexedReaderWithBuffer(IndexedMultiplexSourceWithData source, long _basePosition) :
        IndexedReader(source, _basePosition), IHasDataAsReadOnlyMemory
{
    public ReadOnlyMemory<byte> AsReadOnlyMemory() =>
        source.AsReadOnlyMemory().Slice((int)basePosition);
}

[IntersectionType]
internal readonly partial struct IndexedMultiplexSourceWithData :
    IIntersectionClass<IndexedMultiplexSource>, IHasDataAsReadOnlyMemory
{
}