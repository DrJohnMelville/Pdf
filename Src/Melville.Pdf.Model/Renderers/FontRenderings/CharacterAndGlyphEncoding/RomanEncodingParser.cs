using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;

public static class RomanEncodingParser
{
    public static ValueTask<IByteToCharacterMapping> InterpretEncodingValue(
        PdfObject? encoding, IByteToCharacterMapping? basisEncoding, IGlyphNameMap glyphNames) =>
        (encoding, encoding?.GetHashCode(), basisEncoding) switch
        {
            (null, _, null) => new(CharacterEncodings.Standard),
            (null,_, var basis)  => new (basis),
            (PdfDictionary dict, _, _) => ReadEncodingDictionary(dict, basisEncoding, glyphNames),
            (_, _, not null) => new (basisEncoding),
            (PdfName, KnownNameKeys.WinAnsiEncoding, _) => new(CharacterEncodings.WinAnsi),
            (PdfName, KnownNameKeys.StandardEncoding, _) => new(CharacterEncodings.Standard),
            (PdfName, KnownNameKeys.MacRomanEncoding, _) => new(CharacterEncodings.MacRoman),
            (PdfName, KnownNameKeys.PdfDocEncoding, _) => new(CharacterEncodings.Pdf),
            (PdfName, KnownNameKeys.MacExpertEncoding, _) => new(CharacterEncodings.MacExpert),
            _ => throw new PdfParseException("Invalid encoding member on font.")
        };

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