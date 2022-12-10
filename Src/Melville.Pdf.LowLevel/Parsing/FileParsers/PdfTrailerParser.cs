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

public static class PdfTrailerParser
{
    public static async ValueTask< PdfDictionary> ParseXrefAndTrailer(ParsingFileOwner source, long xrefPosition)
    {
        return await XrefAndTrailer(source, xrefPosition, null).CA();
    }
        
    private static async Task<PdfDictionary> XrefAndTrailer(
        ParsingFileOwner source, long xrefPosition, List<long>? priorPositions)
    {
        PdfDictionary? trailerDictionary;

        var context = await source.RentReader(xrefPosition).CA();
        trailerDictionary = await ReadSingleRefTrailerBlock(context).CA();

        if (trailerDictionary != null)
        {
            await source.InitializeDecryption(trailerDictionary).CA();
        }
        else
        {
            trailerDictionary = await CrossReferenceStreamParser.Read(source, xrefPosition).CA();
        }

        if (trailerDictionary.TryGetValue(KnownNames.Prev, out var prev) && (await prev.CA()) is PdfNumber offset)
        {
            AddToPriorPositions(xrefPosition, ref priorPositions);
            await TryReadPriorTRailer(source, priorPositions, offset.IntValue).CA();
        }
        return  trailerDictionary;
    }

    private static async Task TryReadPriorTRailer(
        ParsingFileOwner source, List<long>? priorPositions, long offsetVal)
    {
        if (priorPositions.Contains(offsetVal)) return;
        await XrefAndTrailer(source, offsetVal, priorPositions).CA();
    }

    private static void AddToPriorPositions(long xrefPosition, ref List<long>? priorPositions)
    {
        priorPositions ??= new List<long>();
        priorPositions.Add(xrefPosition);
    }

    private static async Task<PdfDictionary?> ReadSingleRefTrailerBlock(IParsingReader context)
    {
        if (!await TokenChecker.CheckToken(context.Reader, xrefTag).CA()) return null;
        await NextTokenFinder.SkipToNextToken(context.Reader).CA();
        await new CrossReferenceTableParser(context).Parse().CA();
        await NextTokenFinder.SkipToNextToken(context.Reader).CA();
        if (!await TokenChecker.CheckToken(context.Reader, trailerTag).CA())
            throw new PdfParseException("Trailer does not follow xref");
        var trailer = await context.RootObjectParser.ParseAsync(context).CA();
        if (trailer is not PdfDictionary td)
            throw new PdfParseException("Trailer dictionary is invalid");
        return td;
    }

    private static readonly byte[] xrefTag = {120, 114, 101, 102}; // xref;
    private static readonly byte[] trailerTag = {(byte)'t',114, 97, 105, 108, 101, 114}; // trailer
}