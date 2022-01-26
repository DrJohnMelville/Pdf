using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

public static class PdfTrailerParser
{
    public static async ValueTask< PdfDictionary> ParseXrefAndTrailer(ParsingFileOwner source, long xrefPosition)
    {
        return await XrefAndTrailer(source, xrefPosition).ConfigureAwait(false);
    }
        
    private static async Task<PdfDictionary> XrefAndTrailer(ParsingFileOwner source, long xrefPosition)
    {
        PdfDictionary? trailerDictionary;

        var context = await source.RentReader(xrefPosition).ConfigureAwait(false);
        trailerDictionary = await ReadSingleRefTrailerBlock(context).ConfigureAwait(false);

        if (trailerDictionary != null)
        {
            await source.InitializeDecryption(trailerDictionary).ConfigureAwait(false);
        }
        else
        {
            trailerDictionary = await CrossReferenceStreamParser.Read(source, xrefPosition).ConfigureAwait(false);
        }

        if (trailerDictionary.TryGetValue(KnownNames.Prev, out var prev) && (await prev.ConfigureAwait(false)) is PdfNumber offset)
        {
            await XrefAndTrailer(source, offset.IntValue).ConfigureAwait(false);
        }
        return  trailerDictionary;
    }

    private static async Task<PdfDictionary?> ReadSingleRefTrailerBlock(IParsingReader context)
    {
        if (!await TokenChecker.CheckToken(context.Reader, xrefTag).ConfigureAwait(false)) return null;
        await NextTokenFinder.SkipToNextToken(context.Reader).ConfigureAwait(false);
        await new CrossReferenceTableParser(context).Parse().ConfigureAwait(false);
        await NextTokenFinder.SkipToNextToken(context.Reader).ConfigureAwait(false);
        if (!await TokenChecker.CheckToken(context.Reader, trailerTag).ConfigureAwait(false))
            throw new PdfParseException("Trailer does not follow xref");
        var trailer = await context.RootObjectParser.ParseAsync(context).ConfigureAwait(false);
        if (trailer is not PdfDictionary td)
            throw new PdfParseException("Trailer dictionary is invalid");
        return td;
    }

    private static readonly byte[] xrefTag = {120, 114, 101, 102}; // xref;
    private static readonly byte[] trailerTag = {(byte)'t',114, 97, 105, 108, 101, 114}; // trailer
}