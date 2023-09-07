using System;
using System.IO.Pipelines;
using System.Reflection;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings.BuiltInCMaps;
using Melville.Postscript.Interpreter.Values;
using SingleByteCharacters = Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders.SingleByteCharacters;
using TwoByteCharacters = Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders.TwoByteCharacters;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal readonly partial struct ReadCharacterFactory
{
    [FromConstructor] private readonly PdfFont font;
    [FromConstructor] private readonly PdfEncoding encoding;

    public  ValueTask<IReadCharacter> CreateAsync() =>
        KnownNames.Type0.Equals(font.SubType()) ?
            ParseType0FontEncoding(): 
            new(SingleByteCharacters.Instance);

    private ValueTask<IReadCharacter> ParseType0FontEncoding()
    {
        if (encoding.IsIdentityCdiEncoding()) return new(TwoByteCharacters.Instance);
        return new CMapFactory(
            GlyphNameToUnicodeMap.AdobeGlyphList, 
            HasNoBaseFont.Instance, BuiltinCmapLibrary.Instance).ParseCMapAsync(encoding);
    }
}

[StaticSingleton]
internal partial class HasNoBaseFont: IReadCharacter
{
    public Memory<uint> GetCharacters(
        in ReadOnlyMemory<byte> input, in Memory<uint> scratchBuffer, out int bytesConsumed) => 
        throw new PdfParseException("Builtin CMAPS should not rely on a base font.");
}