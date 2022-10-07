using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

public readonly partial struct ReadCharacterFactory
{
    [FromConstructor] private readonly PdfFont font;
    [FromConstructor] private readonly PdfEncoding encoding;

    public async ValueTask<IReadCharacter> Create()
    {
        return KnownNames.Type0 == await font.SubTypeAsync().CA() ?
           ParseType0FontEncoding(): 
           SingleByteCharacters.Instance;
    }

    private IReadCharacter ParseType0FontEncoding()
    {
        if (!encoding.IsIdentityCdiEncoding())
            throw new NotImplementedException("Only identity CDI encodings are supported");
        return TwoByteCharacters.Instance;
    }
}