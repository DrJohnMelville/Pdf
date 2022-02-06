using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

public static class RandomAccessFileParser
{
    public static Task<PdfLoadedLowLevelDocument> Parse(Stream source) => Parse(new ParsingFileOwner(source));

    public static async Task<PdfLoadedLowLevelDocument> Parse(
        ParsingFileOwner owner, int fileTrailerSizeHint = 30)
    {
        byte major, minor;
        var context = await owner.RentReader(0).CA();
        owner.SetPreheaderOffset(await ConsumeInitialGarbage.CheckForOffset(context.Reader).CA());
        (major, minor) = await PdfHeaderParser.ParseHeadder(context.Reader).CA();

        var xrefPosition = await FileTrailerLocater.Search(owner, fileTrailerSizeHint).CA();
         var dictionary = await PdfTrailerParser.ParseXrefAndTrailer(owner, xrefPosition).CA();
        var firstFree = await owner.IndirectResolver.FreeListHead().CA();

        return new PdfLoadedLowLevelDocument(
            major, minor, dictionary, owner.IndirectResolver.GetObjects(), xrefPosition, firstFree);
    }

}