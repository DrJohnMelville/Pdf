using System;

namespace Melville.Pdf.LowLevel.Encryption
{
    public class PdfSecurityException: Exception
    {
        public PdfSecurityException(string message): base(message)
        {
        }
    }
}