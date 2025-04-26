using System;
using CoreJ2K.Util;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class JPeg200GrayStream : DefaultBaseStream, IImageSizeStream
{
    private readonly PortableImage image;
    private readonly byte[] data;
    private int position;

    public JPeg200GrayStream(PortableImage image) : base(true, false, false)
    {
        this.image = image;
        RawImageCreator.Register();
        data = image.As<RawImage>().Bytes;
    }

    public int Width => image.Width;
    public int Height => image.Height;
    public int ImageComponents => 1;
    public int BitsPerComponent => 8;

    public override int Read(Span<byte> buffer)
    {
        var maxAvail = Math.Clamp(buffer.Length, 0, (data.Length - position)/4);
        for (int i = 0; i < maxAvail; i++)
        {
            buffer[i] = data[position];
            position += 4;
        }
        return maxAvail;
    }
}