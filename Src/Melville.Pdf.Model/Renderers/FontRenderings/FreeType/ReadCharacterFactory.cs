using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public readonly partial struct ReadCharacterFactory
{
    [FromConstructor] private readonly PdfFont font;
    [FromConstructor] private readonly PdfObject? encoding;

    public async ValueTask<IReadCharacter> Create()
    {
        return KnownNames.Type0 == await font.SubTypeAsync().CA() ?
           ParseType0FontEncoding(): 
           SingleByteCharacters.Instance;
    }

    private IReadCharacter ParseType0FontEncoding()
    {
        if (encoding is not PdfName encName)
            throw new NotImplementedException("Only BuiltIn CDI encodings are supported");
        if (!(encName == KnownNames.IdentityH || encName == KnownNames.IdentityV))
            throw new NotImplementedException("Only identity CDI encodings are supported");
        return TwoByteCharacters.Instance;
    }
}