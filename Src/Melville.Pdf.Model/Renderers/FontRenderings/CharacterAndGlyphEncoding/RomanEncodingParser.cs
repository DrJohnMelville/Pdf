using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;

[Obsolete]
public static class RomanEncodingParser
{
    public static ValueTask<IByteToCharacterMapping> InterpretEncodingValue(
        PdfObject? encoding, IByteToCharacterMapping? basisEncoding, IGlyphNameMap glyphNames) =>
        throw new NotImplementedException("this is obsolete");

    private static async ValueTask<IByteToCharacterMapping> ReadEncodingDictionary(
        PdfDictionary dict, IByteToCharacterMapping? basisEncoding, IGlyphNameMap glyphNames)
    {
        var baseEncoding = await InterpretEncodingValue(
            await dict.GetOrDefaultAsync(KnownNames.BaseEncoding,(PdfName?)null).CA(), basisEncoding, glyphNames).CA();
        return dict.TryGetValue(KnownNames.Differences, out var arrTask) &&
               (await arrTask.CA()) is PdfArray arr?
            await CustomFontEncodingFactory.Create(baseEncoding, arr, glyphNames).CA(): baseEncoding;
    }
}