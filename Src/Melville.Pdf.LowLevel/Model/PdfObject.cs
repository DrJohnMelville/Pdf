namespace Melville.Pdf.LowLevel.Model
{
    public abstract class PdfObject
    {
        public virtual PdfObject DirectValue() => this;
    }
}