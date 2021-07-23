using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public static class PdfTrailerParser
    {
        public static async ValueTask<(long FirstFreeBlock, PdfDictionary Dictionary)> ParseXrefAndTrailer(ParsingFileOwner source, long xrefPosition)
        {
            using var context = await source.RentReader(xrefPosition);
            var firstFree = await new CrossReferenceTableParser(context).Parse();
            await NextTokenFinder.SkipToNextToken(context);
          
            if (!await TokenChecker.CheckToken(context, trailerTag)) 
                throw new PdfParseException("Trailer does not follow xref");
            var trailer = await context.RootObjectParser.ParseAsync(context);
            if (trailer is not PdfDictionary trailerDictionery)
                throw new PdfParseException("Trailer dictionary is invalid");
            return (firstFree,trailerDictionery);
        }

        private static readonly byte[] trailerTag = {(byte)'t',114, 97, 105, 108, 101, 114}; // trailer
    }
}