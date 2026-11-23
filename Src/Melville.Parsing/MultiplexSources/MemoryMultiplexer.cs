using Melville.StreamInterfaces;
using System.Buffers;

namespace Melville.Parsing.MultiplexSources;

internal class MemoryMultiplexer(ReadOnlyMemory<byte> data, byte[]? rentedBuffer = null) :
    IndexedMultiplexSource
{
    byte[]? rentedBuffer = rentedBuffer;
    ReadOnlyMemory<byte> data = data;

    protected override void CleanUp()
    {
        if (rentedBuffer is not null)
            ArrayPool<byte>.Shared.Return(rentedBuffer);
        rentedBuffer = null;
        data = default;
    }

    public override long Length => data.Length;

    public override int Read(long position, in Span<byte> buffer)
    {
        var source = data.Span[(int)position..].TakeMax(buffer.Length, out var read);
        source.CopyTo(buffer);
        return read;
    }

    public override ValueTask<int> ReadAsync(
        long position, Memory<byte> buffer, CancellationToken cancellationToken) =>
        ValueTask.FromResult(Read(position, buffer.Span));

    public override IMultiplexSource OffsetFrom(uint offset) =>
        new MemoryMultiplexer(data.Slice((int)offset));

}