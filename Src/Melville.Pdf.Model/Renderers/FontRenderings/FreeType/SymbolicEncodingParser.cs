using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class SymbolicEncodingParser
{
    public static async ValueTask<IGlyphMapping> ParseGlyphMapping(Face face, PdfObject? encoding) =>
        await TryGetDifferenceArray(encoding).CA() is { } diffArray
            ? await DifferenceArrayMapping(face, diffArray).CA()
            : UseFontUnicodeMapping(face);

    private static IGlyphMapping UseFontUnicodeMapping(Face face)
    {
        TrySelectAppleRomanCharMap(face);
        return new UnicodeGlyphMapping(face, PassthroughMapping.Instannce);
    }

    private static ValueTask<PdfArray?> TryGetDifferenceArray(PdfObject? encoding) =>
        encoding is PdfDictionary encodingDict ?
            encodingDict.GetOrDefaultAsync<PdfArray?>(KnownNames.Differences, null):
            new((PdfArray?)null);

    private static async ValueTask<IGlyphMapping> DifferenceArrayMapping(Face face, PdfArray diffArray) =>
        new DictionaryGlyphMapping(await ParseDifferenceArray(face, diffArray).CA());

    private static ValueTask<Dictionary<byte, char>> ParseDifferenceArray(Face face, PdfArray diffArray) => 
        CustomFontEncodingFactory.DifferenceArrayToMapAsync(diffArray, new FreeTypeGlyphNameMap(face));

    private static void TrySelectAppleRomanCharMap(Face face)
    {
        if (face.CharMaps.FirstOrDefault(i=>i.Encoding == Encoding.AppleRoman) is {} charMap)
            face.SetCharmap(charMap);
    }
}