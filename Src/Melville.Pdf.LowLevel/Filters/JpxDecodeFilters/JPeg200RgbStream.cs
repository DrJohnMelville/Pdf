using System;
using Melville.CSJ2K.Util;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class JPeg200RgbStream : DefaultBaseStream, IImageSizeStream
{
    private readonly PortableImage image;
    private int position;

    public JPeg200RgbStream(PortableImage image) : base(true, false, false)
    {
        this.image = image;
    }

    public int Width => image.Width;
    public int Height => image.Height;
    public int ImageComponents => 3;
    public int BitsPerComponent => 8;

    public override int Read(Span<byte> buffer)
    {
        var data = image.Data;
        var maxAvail = (buffer.Length / 3) * 3;
        maxAvail = Math.Clamp(maxAvail, 0, data.Length - position);
        for (int i = 0; i < maxAvail; i+= 3)
        {
            buffer[i + 2] = (byte)(data[position++] * image.ByteScaling[2]);
            buffer[i + 1] = (byte)(data[position++] * image.ByteScaling[1]);
            buffer[i + 0] = (byte)(data[position++] * image.ByteScaling[0]);
        }
        return maxAvail;
    }
}