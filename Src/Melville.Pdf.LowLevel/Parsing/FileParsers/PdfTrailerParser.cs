using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public static async ValueTask< PdfDictionary> ParseXrefAndTrailerAsync(ParsingFileOwner source, long xrefPosition)
    {
        return await XrefAndTrailerAsync(source, xrefPosition, null).CA();
    }
        
    private static async Task<PdfDictionary> XrefAndTrailerAsync(
        ParsingFileOwner source, long xrefPosition, List<long>? priorPositions)
    {
        PdfDictionary? trailerDictionary;

        var context = await source.RentReaderAsync(xrefPosition).CA();
        trailerDictionary = await ReadSingleRefTrailerBlockAsync(context).CA();

        if (trailerDictionary != null)
        {
            await source.InitializeDecryptionAsync(trailerDictionary).CA();
        }
        else
        {
            trailerDictionary = await CrossReferenceStreamParser.ReadAsync(source, xrefPosition).CA();
        }

        if (trailerDictionary.TryGetValue(KnownNames.Prev, out var prev) && (await prev.CA()) is PdfNumber offset)
        {
            AddToPriorPositions(xrefPosition, ref priorPositions);
            await TryReadPriorTRailerAsync(source, priorPositions, offset.IntValue).CA();
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

    private static async Task<PdfDictionary?> ReadSingleRefTrailerBlockAsync(IParsingReader context)
    {
        if (!await TokenChecker.CheckTokenAsync(context.Reader, xrefTag).CA()) return null;
        await NextTokenFinder.SkipToNextTokenAsync(context.Reader).CA();
        await new CrossReferenceTableParser(context).ParseAsync().CA();
        await NextTokenFinder.SkipToNextTokenAsync(context.Reader).CA();
        if (!await TokenChecker.CheckTokenAsync(context.Reader, trailerTag).CA())
            throw new PdfParseException("Trailer does not follow xref");
        var trailer = await context.RootObjectParser.ParseAsync(context).CA();
        if (trailer is not PdfDictionary td)
            throw new PdfParseException("Trailer dictionary is invalid");
        return td;
    }

    private static readonly byte[] xrefTag = {120, 114, 101, 102}; // xref;
    private static readonly byte[] trailerTag = {(byte)'t',114, 97, 105, 108, 101, 114}; // trailer
}