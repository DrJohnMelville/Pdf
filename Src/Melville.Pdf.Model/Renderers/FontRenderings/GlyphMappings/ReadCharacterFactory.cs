using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using SingleByteCharacters = Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders.SingleByteCharacters;
using TwoByteCharacters = Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders.TwoByteCharacters;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal readonly partial struct ReadCharacterFactory
{
    [FromConstructor] private readonly PdfFont font;
    [FromConstructor] private readonly PdfEncoding encoding;

    public IReadCharacter Create()
    {
        return KnownNames.Type0.Equals(font.SubType()) ?
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