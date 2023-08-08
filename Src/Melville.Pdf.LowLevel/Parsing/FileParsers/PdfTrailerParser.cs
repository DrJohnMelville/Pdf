using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal static class PdfTrailerParser
{
    public static Task<PdfDictionary> ParseXrefAndTrailerAsync(ParsingFileOwner source, long xrefPosition) => 
        XrefAndTrailerAsync(source, xrefPosition, null);

    private static async Task<PdfDictionary> XrefAndTrailerAsync(
        ParsingFileOwner source, long xrefPosition, List<long>? priorPositions)
    {

        var context = source.RentReader(xrefPosition);
        var trailerDictionary = await ReadSingleRefTrailerBlockAsync(context).CA();

        if (trailerDictionary != null)
        {
            await source.InitializeDecryptionAsync(trailerDictionary).CA();
        }
        else
        {
            trailerDictionary = await CrossReferenceStreamParser.ReadAsync(source, xrefPosition).CA();
        }

        if ((await trailerDictionary.GetOrDefaultAsync(KnownNames.Prev, -1L).CA()) is var offset && offset >= 0)
        {
            AddToPriorPositions(xrefPosition, ref priorPositions);
            await TryReadPriorTRailerAsync(source, priorPositions, offset).CA();
        }
        return  trailerDictionary;
    }

    private static async Task TryReadPriorTRailerAsync(
        ParsingFileOwner source, List<long>? priorPositions, long offsetVal)
    {
        if (priorPositions?.Contains(offsetVal) ?? false) return;
        await XrefAndTrailerAsync(source, offsetVal, priorPositions).CA();
    }

    private static void AddToPriorPositions(long xrefPosition, ref List<long>? priorPositions)
    {
        priorPositions ??= new List<long>();
        priorPositions.Add(xrefPosition);
    }

    private static async Task<PdfDictionary?> ReadSingleRefTrailerBlockAsync(ParsingReader context)
    {
        if (!await TokenChecker.CheckTokenAsync(context.Reader, xrefTag).CA()) return null;
        await NextTokenFinder.SkipToNextTokenAsync(context.Reader).CA();
        await new CrossReferenceTableParser(context.Reader, context.Owner.NewIndirectResolver).ParseAsync().CA();
        await NextTokenFinder.SkipToNextTokenAsync(context.Reader).CA();
        if (!await TokenChecker.CheckTokenAsync(context.Reader, trailerTag).CA())
            throw new PdfParseException("Trailer does not follow xref");
        var trailerIndirect = (await new RootObjectParser(context).ParseAsync().CA());
        if (!trailerIndirect.TryGetEmbeddedDirectValue(out PdfDictionary? trailer))
            throw new PdfParseException("Trailer dictionary is invalid");
        return trailer;
    }

    private static readonly byte[] xrefTag = {120, 114, 101, 102}; // xref;
    private static readonly byte[] trailerTag = {(byte)'t',114, 97, 105, 108, 101, 114}; // trailer
}