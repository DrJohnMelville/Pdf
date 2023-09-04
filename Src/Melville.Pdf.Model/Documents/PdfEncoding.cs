using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

namespace Melville.Pdf.Model.Documents;

internal readonly partial struct PdfEncoding
{
    [FromConstructor] public PdfDirectObject LowLevel { get; }

    public bool IsIdentityCdiEncoding() =>
        LowLevel.Equals(KnownNames.IdentityH) ||
        LowLevel.Equals(KnownNames.IdentityV);
}