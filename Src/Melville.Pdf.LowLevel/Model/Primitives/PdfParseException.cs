using System;

namespace Melville.Pdf.LowLevel.Model.Primitives;

/// <summary>
/// Pdf Parse Exceptions occur when a document violates the PDF specification and the parser cannot continue.
/// </summary>
public class PdfParseException : Exception
{
    /// <summary>
    /// Constructs a Pdf Parse Exception
    /// </summary>
    /// <param name="message"></param>
    public PdfParseException(string message) : base(message)
    {
    }
}