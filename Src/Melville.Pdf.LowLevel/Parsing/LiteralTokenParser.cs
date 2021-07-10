using System.Buffers;
using Melville.Pdf.LowLevel.Model;

namespace Melville.Pdf.LowLevel.Parsing
{
    public class LiteralTokenParser : PdfAtomParser
    {
        private int length;
        private PdfObject literal;

        public LiteralTokenParser(int length, PdfObject literal)
        {
            this.length = length;
            this.literal = literal;
        }

        public override bool TryParse(ref SequenceReader<byte> reader, out PdfObject obj)
        {
            obj = literal;
            return reader.TryAdvance(length);
        }
    }
}