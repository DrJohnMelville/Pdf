namespace Melville.Pdf.LowLevel.Parsing.StringParsing
{
    public class ByteCounter : LiteralStringParserTarget
    {
        public int Length { get; private set; }
        public override void AddByte(byte item) => Length++;
    }
}