using Melville.StreamInterfaces;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

[Obsolete("Can get rid of this, too.")]
internal class StraightCopyStream : RentedArrayReadingStream
{
    public StraightCopyStream(byte[] data, int length) : base(data, length)
    {
    }

    protected override int CopyBytes(Span<byte> source, Span<byte> destination)
    {
        source.TakeMax(destination.Length, out var len).CopyTo(destination);
        return len;
    }
}