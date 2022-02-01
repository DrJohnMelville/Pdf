using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;

public static class NonsymbolicEncodingParser
{
    public static ValueTask<IByteToUnicodeMapping> InterpretEncodingValue(PdfObject encoding) =>
        (encoding, encoding.GetHashCode()) switch
        {
            (_, KnownNameKeys.WinAnsiEncoding) => new(CharacterEncodings.WinAnsi),
            (var x,_) when x == PdfTokenValues.Null => new (CharacterEncodings.Standard),
            (_, KnownNameKeys.StandardEncoding) => new(CharacterEncodings.Standard),
            (_, KnownNameKeys.MacRomanEncoding) => new(CharacterEncodings.MacRoman),
            (_, KnownNameKeys.PdfDocEncoding) => new(CharacterEncodings.Pdf),
            (_, KnownNameKeys.MacExpertEncoding) => new(CharacterEncodings.MacExpert),
            (PdfDictionary dict, _) => ReadEncodingDictionary(dict),
            _ => throw new PdfParseException("Invalid encoding member on font.")
        };

    private static async ValueTask<IByteToUnicodeMapping> ReadEncodingDictionary(PdfDictionary dict)
    {
        var baseEncoding = dict.TryGetValue(KnownNames.BaseEncoding, out var baseTask)
            ? await InterpretEncodingValue(await baseTask.CA()).CA()
            : CharacterEncodings.Standard;
        return dict.TryGetValue(KnownNames.Differences, out var arrTask) &&
               (await arrTask.CA()) is PdfArray arr?
            await CustomFontEncodingFactory.Create(baseEncoding, arr).CA(): baseEncoding;
    }
}