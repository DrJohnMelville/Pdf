namespace Melville.Pdf.LowLevel.Model
{
    public class PdfArray : PdfObject
    {
        public PdfArray(PdfObject[] items)
        {
            Items = items;
        }

        public PdfObject[] Items { get; }
    }
}