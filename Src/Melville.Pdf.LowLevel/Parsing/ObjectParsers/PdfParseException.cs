using System;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public class PdfParseException : Exception
    {
        public PdfParseException(string? message) : base(message)
        {
        }
    }
}