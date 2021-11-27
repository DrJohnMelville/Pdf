using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

public static class PdfHeaderParser
{
    public static async ValueTask<(byte Major, byte Minor)> ParseHeadder(IPipeReaderWithPosition context)
    {
        byte major, minor;
        do {} while(context.ShouldContinue(ParseDocumentHeader(await context.ReadAsync(),
                        out major, out minor)));

        return (major, minor);
    }
        
    public static (bool Success, SequencePosition Position) ParseDocumentHeader(
        ReadResult src, out byte majorVersion, out byte minorVersion)
    {
        var seq = src.Buffer;
        majorVersion = minorVersion = 0;
        var reader = new SequenceReader<byte>(seq);
        if (!reader.TryCheckToken(headerTemplate, src.IsCompleted, out var hasHeader)) 
            return (false, reader.Position);
        if (!hasHeader)
            throw new PdfParseException("File does not begin with a PDF header");
        if (!(reader.TryRead(out var majorByte) &&
              reader.TryRead(out var periodByte) &&
              reader.TryRead(out var minorByte))) return (false, reader.Position);
        if (!(CheckByte(majorByte, out majorVersion) && periodByte == '.' &&
              CheckByte(minorByte, out minorVersion)))
            throw new PdfParseException("Invalid pdf version number.");
        return (true, reader.Position);
    }
        
    private static bool CheckByte(byte b, out byte output)
    {
        output = (byte) (b - '0');
        return output is >= 0 and <= 9;
    }

    private static byte[] headerTemplate = new byte[] {37, 80, 68, 70, 45}; // %PDF-;
}