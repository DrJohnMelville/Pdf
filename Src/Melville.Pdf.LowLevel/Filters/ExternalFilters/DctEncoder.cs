using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Melville.Pdf.LowLevel.Filters.ExternalFilters;

public class DctDecoder : ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
    {
        var img = await Image.LoadAsync<Rgb24>(input);
        return new ImageReadStream(img);
    }

}

public class ImageReadStream : DefaultBaseStream
{
    private Image<Rgb24> source;
    private int currentRow;
    private int currentByte;
    public ImageReadStream(Image<Rgb24> source) : base(true, false, false)
    {
        this.source = source;
    }

    public override int Read(Span<byte> buffer)
    {
        var totalLen = 0;
        while (true)
        {
            if (buffer.Length == 0) return totalLen;
            if (currentRow > source.Height) return totalLen;
            var localLen = TryReadFromRow(buffer);
            totalLen += localLen;
            buffer = buffer.Slice(localLen);
        }
    }

    private int TryReadFromRow(Span<byte> buffer)
    {
        var rowSpan = source.GetPixelRowSpan(currentRow);
        var byteSpan = MemoryMarshal.AsBytes(rowSpan).Slice(currentByte);
        var bytesToCopy = CopyLen(byteSpan.Length, buffer.Length);
        byteSpan.Slice(0,bytesToCopy).CopyTo(buffer);
        return bytesToCopy;
    }

    private int CopyLen(int sourceLength, int destLength)
    {
        if (sourceLength <= destLength)
        {
            currentByte = 0;
            currentRow++;
            return sourceLength;
        }
        else
        {
            currentByte += destLength;
            return destLength;
        }
    }
}