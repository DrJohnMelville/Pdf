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

public class YCrCbStream: RentedArrayReadingStream
{
    public YCrCbStream(byte[] data, int length) : base(data, length)
    {
    }

    protected override int CopyBytes(Span<byte> source, Span<byte> destination)
    {
        var minLen = Math.Min(source.Length, destination.Length) / 3; // integer division
        JpegYCbCrToRgbConverter.Shared.ConvertYCbCr8ToRgb24(source, destination, minLen);
        return minLen * 3;
    }
}