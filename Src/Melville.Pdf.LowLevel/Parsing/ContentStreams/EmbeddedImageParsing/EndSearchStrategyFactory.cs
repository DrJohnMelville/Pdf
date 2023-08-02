using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal static class EndSearchStrategyFactory
{
    public static EndSearchStrategy Create(DictionaryBuilder dict)
    {
        if (!TryGetLength(dict, out var length)) return EndSearchStrategy.Instance;
        if (dict.TryGetValue(KnownNames.Filter, out var arrItem) && IsAsciiFilter(arrItem.Value))
            return new WhiteSpaceAndLengthSearchStrategy(length);
        else
            return new WithLengthSearchStrategy(length);
    }

    private static bool IsAsciiFilter(PdfIndirectObject arrItem) => 
        arrItem.TryGetEmbeddedDirectValue(out var dirval) && dirval switch
    {
        _ when dirval.Equals(KnownNames.A85) ||
               dirval.Equals(KnownNames.AHx) ||
               dirval.Equals(KnownNames.ASCII85Decode) ||
               dirval.Equals(KnownNames.ASCIIHexDecode) => true,
        _ when dirval.TryGet(out PdfArray? arr) && arr.Count > 0 => 
            IsAsciiFilter(arr.RawItems[0]),
        _ => false
    };

    private static bool TryGetLength(DictionaryBuilder dict, out int length) =>
        dict.TryGetValue(KnownNames.Length, out var item) &&
        item.Value.TryGetEmbeddedDirectValue(out var dirLength) &&
        dirLength.TryGet(out length) ||
        0.AsFalseValue(out length);
}