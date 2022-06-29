using System;
using System.IO;
using System.Threading.Tasks;
using Melville.CSJ2K;
using Melville.CSJ2K.Util;
using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

public class JpxToPdfAdapter: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters)
    {
        throw new System.NotSupportedException();
    }

    public ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
    {
        var independentImage = J2kImage.FromStream(input);
        return new(new JPeg200Stream(independentImage));
    }
}

public class JPeg200Stream : DefaultBaseStream, IImageSizeStream
{
    private readonly PortableImage image;
    private int position;

    public JPeg200Stream(PortableImage image) : base(true, false, false)
    {
        this.image = image;
    }

    public int Width => image.Width;
    public int Height => image.Height;

    public override int Read(Span<byte> buffer)
    {
        var maxAvail = (buffer.Length / 3) * 3;
        maxAvail = Math.Clamp(maxAvail, 0, image.Data.Length - position);
        for (int i = 0; i < maxAvail; i+= 3)
        {
            buffer[i + 2] = (byte)(image.Data[position++] * image.ByteScaling[2]);
            buffer[i + 1] = (byte)(image.Data[position++] * image.ByteScaling[1]);
            buffer[i + 0] = (byte)(image.Data[position++] * image.ByteScaling[0]);
        }

        return maxAvail;
    }
}