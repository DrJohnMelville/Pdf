using System.Buffers;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
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