using System;

namespace Melville.Pdf.LowLevel.Model.Primitives
{
    public class PdfParseException : Exception
    {
        public PdfParseException(string message) : base(message)
        {
        }
    }
}