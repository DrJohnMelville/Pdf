namespace Melville.Pdf.LowLevel.Model
{
    public sealed class PdfNull: PdfObject
    {
        public static readonly PdfNull Instance = new();
        private PdfNull() {}
    }
}