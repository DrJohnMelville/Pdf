using Melville.IntersectionTypes;
using Melville.StreamInterfaces;
using System.Buffers;

namespace Melville.Parsing.MultiplexSources;

internal class SingleBufferSeekableMultiplexSource : IndexedMultiplexSource, IHasDataAsReadOnlyMemory
{
    private int nextRead;
    private byte[] sourceBuffer;
    private readonly ReaderWithLength source;

    public SingleBufferSeekableMultiplexSource(ReaderWithLength source)
    {
        this.source = source;
        sourceBuffer = ArrayPool<byte>.Shared.Rent((int)source.Length);
        Length = source.Length;
    }

    public override long Length { get; }

    public ReadOnlyMemory<byte> AsReadOnlyMemory()
    {
        EnsureHasBytes(Length);
        return sourceBuffer.AsMemory(0, (int)Length);
    }

    void EnsureHasBytes(long desired)
    {
        while (nextRead < desired && source.Read(sourceBuffer.AsSpan(nextRead)) is > 0 and var read)
            nextRead += read;
    }
    async ValueTask EnsureHasBytesAsync(long desired, CancellationToken ct)
    {
        while (nextRead < desired &&
            (await source.ReadAsync(sourceBuffer.AsMemory(nextRead), ct).CA()) is > 0 and var read)
            nextRead += read;
    }
    public override int Read(long position, in Span<byte> buffer)
    {
        EnsureHasBytes(position + buffer.Length);
        return CopyBytesToDestination(position, buffer);
    }

    private int CopyBytesToDestination(long position, Span<byte> buffer)
    {
        var sourceSlice = sourceBuffer.AsSpan()[(int)position..nextRead].TakeMax(buffer.Length, out var length);
        sourceSlice.CopyTo(buffer);
        return length;
    }

    public override async ValueTask<int> ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        await EnsureHasBytesAsync(position + buffer.Length, cancellationToken).CA();
        return CopyBytesToDestination(position, buffer.Span);
    }

    protected override void CleanUp()
    {
        if (sourceBuffer.Length is 0) return;
        ArrayPool<byte>.Shared.Return(sourceBuffer);
        sourceBuffer = [];
    }
}


[IntersectionType]
internal readonly partial struct ReaderWithLength : IAsyncReader, ISyncReader, IStreamLength
{

}