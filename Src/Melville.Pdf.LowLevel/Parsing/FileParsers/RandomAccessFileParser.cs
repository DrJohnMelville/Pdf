using Melville.Parsing;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal static class RandomAccessFileParser
{
    private static readonly SearchValues<byte> findPercent = SearchValues.Create("%"u8);
    
    public static async ValueTask<PdfLoadedLowLevelDocument> ParseAsync(
        ParsingFileOwner owner, int fileTrailerSizeHint = 30)
    {
        using var _ = Force.Using(owner.SubsetIAsyncReader(0), out var reader);
        using var buffer = new ParsingBuffer(reader);
        var preheaderOffset = await buffer.EnsureHasCharAsync(findPercent).CA();
        owner.SetPreheaderOffset(preheaderOffset);
        buffer.Advance(preheaderOffset);

        await buffer.EnsureBytesAsync(8);
        var (major, minor) = ParseHeader(buffer.Peek());

        var xrefPosition = await FileTrailerLocater.SearchAsync(owner, fileTrailerSizeHint).CA();
        var dictionary = await new PdfTrailerParser(owner, xrefPosition).ParseXrefAndTrailerAsync().CA();

        return new PdfLoadedLowLevelDocument(
           major, minor, dictionary, owner.IndirectResolver.GetObjects(), xrefPosition, owner);
    }

    static (byte major, byte minor) ParseHeader(ReadOnlySpan<byte> header)
    {
        if (!(header.StartsWith("%PDF-"u8) && header[6] is (byte)'.'))
            throw new PdfParseException("File does not begin with a PDF header");
        var major = CharacterClassifier.ValueFromDigit(header[5]);
        var minor = CharacterClassifier.ValueFromDigit(header[7]);
        if (major > 9 || minor > 9)
            throw new PdfParseException("Invalid pdf version number.");
        return (major, minor);
    }
}