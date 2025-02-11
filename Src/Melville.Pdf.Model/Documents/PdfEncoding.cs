using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// Costume type for a PDF font encoding object
/// </summary>
/// <param name="LowLevel"></param>
public record struct PdfEncoding(PdfDirectObject LowLevel)
{
    /// <summary>
    /// Check if this is an Identity Cdi Encoding
    /// </summary>
    /// <returns>True if it represents an identity encoding false otherwise</returns>
    public bool IsIdentityCdiEncoding() =>
        LowLevel.Equals(KnownNames.IdentityH) ||
        LowLevel.Equals(KnownNames.IdentityV);
}