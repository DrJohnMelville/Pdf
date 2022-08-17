using System;
using System.Buffers;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public interface IJpegBlockParser
{
    public void ParseBlock(SequenceReader<byte> data, JpegStreamFactory factory);
}

[StaticSingleton]
public sealed partial class IgnoreBlockParser: IJpegBlockParser
{
    public void ParseBlock(SequenceReader<byte> data, JpegStreamFactory factory)
    {
        // do nothing
    }
}

[StaticSingleton]
public sealed partial class VerifyApp0Segment: IJpegBlockParser
{
    public void ParseBlock(SequenceReader<byte> data, JpegStreamFactory factory)
    {
        Span<byte> magicString = stackalloc byte[] { 0x4a, 0x46, 0x49, 0x46,0 }; // JFIF\0
        Span<byte> readString = stackalloc byte[5];
        if (!(data.TryCopyTo(readString) && magicString.SequenceEqual(readString)))
            throw new PdfParseException("Jpeg image is missing JFIF Magic Number in App0 Segment");
        Debug.WriteLine("  App0 Segment is valid");
    }
}