using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public class RandomAccessFileParser
    {
        private readonly ParsingSource context;

        public RandomAccessFileParser(ParsingSource context)
        {
            this.context = context;
        }

        public async Task<PdfLowLevelDocument> Parse(int fileTrailerSizeHint = 1024)
        {
            CheckBeginAtPositionZero();
            byte major, minor;
            do { } while (context.ShouldContinue(
                PdfHeaderParser.ParseDocumentHeader(await context.ReadAsync(), out major, out minor)));

            var dictioary = await ParseTrailer.Parse(context, fileTrailerSizeHint);
            return new PdfLowLevelDocument(major, minor, dictioary);
        }
        
        private void CheckBeginAtPositionZero()
        {
            if (context.Position != 0)
                throw new PdfParseException("Parsing must begin at position 0.");
        }
    }
}