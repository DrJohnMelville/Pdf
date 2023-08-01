using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.Model.Documents;

internal readonly partial struct PdfEncoding
{
    [FromConstructor] public PdfDirectValue LowLevel { get; }

    public bool IsIdentityCdiEncoding() =>
        LowLevel.Equals(KnownNames.IdentityHTName) ||
        LowLevel.Equals(KnownNames.IdentityVTName);
}