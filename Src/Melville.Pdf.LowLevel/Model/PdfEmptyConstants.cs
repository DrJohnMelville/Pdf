namespace Melville.Pdf.LowLevel.Model
{
    public sealed class PdfEmptyConstants: PdfObject
    {
        public static readonly PdfEmptyConstants Null = new();
        // These are not part of the PDF spec -- they are sentinels for a parser implementation trick;
        public static readonly PdfEmptyConstants ArrayTerminator = new();
        public static readonly PdfEmptyConstants DictionaryTerminator = new();
        private PdfEmptyConstants() {}
    }
}