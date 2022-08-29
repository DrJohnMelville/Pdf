namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

public class StraightCopyStream : RentedArrayReadingStream
{
    public StraightCopyStream(byte[] data, int length) : base(data, length)
    {
    }

    protected override int CopyBytes(Span<byte> source, Span<byte> destination)
    {
        var bytesToCopy = Math.Min(source.Length, destination.Length);
        source.Slice(0,bytesToCopy).CopyTo(destination);
        return bytesToCopy;
    }
}