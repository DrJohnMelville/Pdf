using System.Threading.Tasks;

using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal static class RandomAccessFileParser
{
    public static async ValueTask<PdfLoadedLowLevelDocument> ParseAsync(
        ParsingFileOwner owner, int fileTrailerSizeHint = 30)
    {
        byte major, minor;
        using var context = owner.SubsetReader(0);
        owner.SetPreheaderOffset(await ConsumeInitialGarbage.CheckForOffsetAsync(context.ObsoleteReader).CA());
        (major, minor) = await PdfHeaderParser.ParseHeadderAsync(context.ObsoleteReader).CA();

        var xrefPosition = await FileTrailerLocater.SearchAsync(owner, fileTrailerSizeHint).CA();
         var dictionary = await new PdfTrailerParser(owner, xrefPosition).ParseXrefAndTrailerAsync().CA();

         return new PdfLoadedLowLevelDocument(
            major, minor, dictionary, owner.NewIndirectResolver.GetObjects(), xrefPosition, owner);
    }
}