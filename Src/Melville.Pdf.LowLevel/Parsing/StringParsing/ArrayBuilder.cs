namespace Melville.Pdf.LowLevel.Parsing.StringParsing
{
    public class ArrayBuilder: LiteralStringParserTarget
    {
        public byte[] Result { get; }
        private int position = 0;

        public ArrayBuilder(int length)
        {
            Result = new byte[length];
        }

        public override void AddByte(byte item) => Result[position++] = item;
    }
}