namespace Melville.Pdf.LowLevel.Model
{
    public sealed class PdfNull: PdfObject
    {
        public static readonly PdfNull Instance = new();
        // These are not part of the PDF spec -- they are sentinels for a parser implementation trick;
        public static readonly PdfNull ArrayTerminator = new();
        public static readonly PdfNull DictionaryTerminator = new();
        private PdfNull() {}
    }
}