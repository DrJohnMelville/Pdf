using System;
using Melville.CSJ2K.Util;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

public class JPeg200GrayStream : DefaultBaseStream, IImageSizeStream
{
    private readonly PortableImage image;
    private int position;

    public JPeg200GrayStream(PortableImage image) : base(true, false, false)
    {
        this.image = image;
    }

    public int Width => image.Width;
    public int Height => image.Height;
    public int ImageComponents => 1;
    public int BitsPerComponent => 8;

    public override int Read(Span<byte> buffer)
    {
        var data = image.Data;
        var maxAvail = Math.Clamp(buffer.Length, 0, data.Length - position);
        for (int i = 0; i < maxAvail; i++)
        {
            buffer[i + 0] = (byte)(data[position++] * image.ByteScaling[0]);
        }
        return maxAvail;
    }
}