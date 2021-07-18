using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public static class RandomAccessFileParser
    {
        public static async Task<PdfLowLevelDocument> Parse(
            ParsingFileOwner owner, int fileTrailerSizeHint = 30)
        {
            byte major, minor;
            using (var context = await owner.RentReader(0))
            {
                (major, minor) = await PdfHeaderParser.ParseHeadder(context);
            }

            var xrefPosition = await FileTrailerLocater.Search(owner, fileTrailerSizeHint);
            var dictionary = await PdfTrailerParser.ParseXrefAndTrailer(owner, xrefPosition);
            
            return new PdfLoadedLowLevelDocument(
                major, minor, dictionary, owner.IndirectResolver.GetObjects(), xrefPosition);
        }
        
    }
}