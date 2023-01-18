using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

internal readonly partial struct PdfEncoding
{
    [FromConstructor] public PdfObject LowLevel { get; }

    private PdfName? EncodingAsName() => LowLevel as PdfName;

    public bool IsIdentityCdiEncoding() =>
        EncodingAsName() is { } name &&
        (name == KnownNames.IdentityH || name == KnownNames.IdentityV);
}