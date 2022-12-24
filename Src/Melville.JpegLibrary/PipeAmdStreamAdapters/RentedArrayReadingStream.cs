using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.Streams.Bases;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

internal abstract class RentedArrayReadingStream : DefaultBaseStream
{
    private readonly byte[] data;
    private int length;
    private int position;
    public RentedArrayReadingStream(byte[] data, int length) : base(true, false, false)
    {
        this.data = data;
        this.length = length;
        Debug.Assert(this.data.Length >= this.length);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        var ret = CopyBytes(data.AsSpan(position), buffer.Span);
        position += ret;
        return new(ret);
    }

    protected abstract int CopyBytes(Span<byte> source, Span<byte> destination);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ArrayPool<byte>.Shared.Return(data);
        }
    }
}