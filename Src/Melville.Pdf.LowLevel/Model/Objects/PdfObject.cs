namespace Melville.Pdf.LowLevel.Model.Objects
{
    public abstract class PdfObject
    {
        public virtual PdfObject DirectValue() => this;
    }
}