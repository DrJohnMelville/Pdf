using System;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public class PdfSecurityException: Exception
    {
        public PdfSecurityException(string message): base(message)
        {
        }
    }
}