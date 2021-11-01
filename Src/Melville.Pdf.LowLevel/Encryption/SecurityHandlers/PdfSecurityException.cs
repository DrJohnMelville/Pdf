using System;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

public class PdfSecurityException: PdfParseException
{
    public PdfSecurityException(string message): base(message)
    {
    }
}