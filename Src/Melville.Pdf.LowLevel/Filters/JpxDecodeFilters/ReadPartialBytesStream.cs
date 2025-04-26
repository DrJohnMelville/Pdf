using System;
using CoreJ2K.Util;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class ReadPartialBytesStream: DefaultBaseStream, IImageSizeStream
{
    private readonly PortableImage image;
    private readonly int bytesPerQuad;
    private readonly int skipPerQuad;
    private readonly byte[] data;
    private int position;

    public ReadPartialBytesStream(PortableImage image, int bytesPerQuad) : base(true, false, false)
    {
        this.image = image;
        this.bytesPerQuad = bytesPerQuad;
        skipPerQuad = 4 - bytesPerQuad;
        RawImageCreator.Register();
        data = image.As<RawImage>().Bytes;
    }

    public int Width => image.Width;
    public int Height => image.Height;
    public int ImageComponents => bytesPerQuad;
    public int BitsPerComponent => 8;

    public override int Read(Span<byte> buffer)
    {
        int target = 0;
        while (position < data.Length && target < buffer.Length)
        {
            int sourcelen = bytesPerQuad - (position % 4);
            var targetFinalLen = Math.Min(buffer.Length - target, sourcelen);
            data.AsSpan(position, targetFinalLen).CopyTo(buffer[target..]);
            target += targetFinalLen;
            position += targetFinalLen;
            if (sourcelen == targetFinalLen) position += (skipPerQuad);
        }

        return target;
    }
}