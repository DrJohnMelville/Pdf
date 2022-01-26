using System.IO;
using System.Threading.Tasks;
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
        var context = await owner.RentReader(0).ConfigureAwait(false);
        owner.SetPreheaderOffset(await ConsumeInitialGarbage.CheckForOffset(context.Reader).ConfigureAwait(false));
        (major, minor) = await PdfHeaderParser.ParseHeadder(context.Reader).ConfigureAwait(false);

        var xrefPosition = await FileTrailerLocater.Search(owner, fileTrailerSizeHint).ConfigureAwait(false);
        var dictionary = await PdfTrailerParser.ParseXrefAndTrailer(owner, xrefPosition).ConfigureAwait(false);
        var firstFree = await owner.IndirectResolver.FreeListHead().ConfigureAwait(false);

        return new PdfLoadedLowLevelDocument(
            major, minor, dictionary, owner.IndirectResolver.GetObjects(), xrefPosition, firstFree);
    }

}