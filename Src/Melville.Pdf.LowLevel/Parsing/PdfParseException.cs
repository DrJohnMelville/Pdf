using System;

namespace Melville.Pdf.LowLevel.Parsing
{
    public class PdfParseException : Exception
    {
        public PdfParseException(string? message) : base(message)
        {
        }
    }
}