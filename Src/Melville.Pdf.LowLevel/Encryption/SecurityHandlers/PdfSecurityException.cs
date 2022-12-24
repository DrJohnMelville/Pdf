using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

/// <summary>
/// Exception thrown when a file cannot be decrypted due to a problems in the security subsystem.
/// </summary>
public class PdfSecurityException: PdfParseException
{
    /// <summary>
    /// Create a new exception object.
    /// </summary>
    /// <param name="message">A meaasge explaining what went wrong.</param>
    public PdfSecurityException(string message): base(message)
    {
    }
}