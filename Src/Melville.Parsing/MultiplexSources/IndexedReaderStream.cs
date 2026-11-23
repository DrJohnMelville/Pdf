using Melville.IntersectionTypes;
using Melville.Parsing.Streams.Bases;
using Melville.StreamInterfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Melville.Parsing.MultiplexSources;


internal class IndexedReaderStreamWithBuffer(IndexedMultiplexSourceWithData source, long position) :
    IndexedReaderStream(source, position), IHasDataAsReadOnlyMemory, IMultiplexSource
{
    public ReadOnlyMemory<byte> AsReadOnlyMemory() =>
        source.AsReadOnlyMemory().Slice((int)basePosition);
}

internal class IndexedReaderStream : 
    DefaultBaseStream, IHasSourceTicket
{
    protected readonly long basePosition;
    private IndexedMultiplexSource? source = null;
    CountedSourceTicket IHasSourceTicket.SourceTicket { get; set; }

    public IndexedReaderStream(IndexedMultiplexSource source, long position):
        base (true, false, true)
    {
        this.source = source;
        basePosition = position;
    }

    public override int Read(Span<byte> buffer)
    {
        VerifyInitialized();
        var ret = source.Read(Position+basePosition, buffer);
        Position += ret;
        return ret;
    }

    [MemberNotNull(nameof(source))]
    private void VerifyInitialized()
    {
        if (source is null) 
            throw new InvalidOperationException($"IndexedReaderStream has no source.");
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        VerifyInitialized();
        var ret = await source.ReadAsync(Position+basePosition, buffer, cancellationToken).CA();
        Position += ret;
        return ret;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPosition = SeekTargetFromStreamStart(offset, origin);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)newPosition, (ulong)Length);
        return Position = newPosition;
    }

    private long SeekTargetFromStreamStart(long offset, SeekOrigin origin) =>
        origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => (source?.Length??0) + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };

    public override long Length => (source?.Length ?? 0) - basePosition;

    protected override void Dispose(bool disposing)
    {
        if (source is null) return; // prevents recursion
        source = null;
        ((IHasSourceTicket)this).SourceTicket.Release(this);
        base.Dispose(disposing);
    }

    public Stream ReadFrom(long position) => source.ReadFrom(basePosition + position);

    public IMultiplexReader ReaderFrom(long position) => source.ReaderFrom(basePosition + position);

    public IMultiplexSource OffsetFrom(uint offset) => source.OffsetFrom((uint)(basePosition + Position));

}
