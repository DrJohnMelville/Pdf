using System;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers
{
    public class PdfSecurityException: Exception
    {
        public PdfSecurityException(string message): base(message)
        {
        }
    }
}