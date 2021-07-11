namespace Melville.Pdf.LowLevel.Model.Objects
{
    public sealed class PdfBoolean : PdfObject
    {
        public bool Value => this == True;

        private PdfBoolean()
        {
        }

        public static readonly PdfBoolean True = new();
        public static readonly PdfBoolean False = new();
    }
}