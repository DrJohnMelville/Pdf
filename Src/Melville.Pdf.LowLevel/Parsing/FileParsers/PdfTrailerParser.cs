using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public static class PdfTrailerParser
    {
        public static async ValueTask< PdfDictionary> ParseXrefAndTrailer(ParsingFileOwner source, long xrefPosition)
        {
            return await XrefAndTrailer(source, xrefPosition);
        }

        private static async Task<PdfDictionary> XrefAndTrailer(ParsingFileOwner source, long xrefPosition)
        {
            PdfDictionary? trailerDictionery;
            using (var context = await source.RentReader(xrefPosition))
            {
                await new CrossReferenceTableParser(context).Parse();
                await NextTokenFinder.SkipToNextToken(context);

                if (!await TokenChecker.CheckToken(context, trailerTag))
                    throw new PdfParseException("Trailer does not follow xref");
                var trailer = await context.RootObjectParser.ParseAsync(context);
                if (trailer is not PdfDictionary td)
                    throw new PdfParseException("Trailer dictionary is invalid");
                trailerDictionery = td;
            }

            if (trailerDictionery.TryGetValue(KnownNames.Prev, out var prev) && (await prev) is PdfNumber offset)
            {
                await XrefAndTrailer(source, offset.IntValue);
            }
            return  trailerDictionery;
        }

        private static readonly byte[] trailerTag = {(byte)'t',114, 97, 105, 108, 101, 114}; // trailer
    }
}