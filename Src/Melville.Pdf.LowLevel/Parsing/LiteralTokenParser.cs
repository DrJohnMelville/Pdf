using System.Buffers;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.NameParsing;

namespace Melville.Pdf.LowLevel.Parsing
{
    public class LiteralTokenParser : IPdfObjectParser
    {
        private int length;
        private PdfObject literal;

        public LiteralTokenParser(int length, PdfObject literal)
        {
            this.length = length;
            this.literal = literal;
        }

        public bool TryParse(ref SequenceReader<byte> reader, out PdfObject obj)
        {
            obj = literal;
            return reader.TryAdvance(length) && NextTokenFinder.SkipToNextToken(ref reader);
        }
    }
}