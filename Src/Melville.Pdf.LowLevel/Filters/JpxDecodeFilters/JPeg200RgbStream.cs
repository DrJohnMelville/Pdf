using System;
using CoreJ2K.Util;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class JPeg200RgbStream : DefaultBaseStream, IImageSizeStream
{
    private readonly PortableImage image;
    private readonly byte[] data;
    private int position;

    public JPeg200RgbStream(PortableImage image) : base(true, false, false)
    {
        this.image = image;
        RawImageCreator.Register();
        data = image.As<RawImage>().Bytes;
    }

    public int Width => image.Width;
    public int Height => image.Height;
    public int ImageComponents => 3;
    public int BitsPerComponent => 8;

    public override int Read(Span<byte> buffer)
    {
        int target = 0;
        while (position < data.Length && target < buffer.Length)
        {
            int sourcelen = 3 - (position % 4);
            var targetFinalLen = Math.Min(buffer.Length - target, sourcelen);
            data.AsSpan(position, targetFinalLen).CopyTo(buffer[target..]);
            target += targetFinalLen;
            position += targetFinalLen;
            if (sourcelen == targetFinalLen) position++;
        }

        return target;
    }
}