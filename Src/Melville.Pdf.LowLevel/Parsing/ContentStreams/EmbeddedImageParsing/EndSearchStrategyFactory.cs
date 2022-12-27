using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal static class EndSearchStrategyFactory
{
    public static EndSearchStrategy Create(DictionaryBuilder dict)
    {
        if (!TryGetLength(dict, out var length)) return EndSearchStrategy.Instance;
        if (dict.TryGetValue(KnownNames.Filter, out var arrItem) && IsAsciiFilter(arrItem))
            return new WhiteSpaceAndLengthSearchStrategy(length);
        else
            return new WithLengthSearchStrategy(length);
    }

    private static bool IsAsciiFilter(PdfObject arrItem) => arrItem switch
    {
        PdfName name when name.GetHashCode() is KnownNameKeys.A85 or KnownNameKeys.AHx
            or KnownNameKeys.ASCII85Decode or KnownNameKeys.ASCIIHexDecode => true,
        PdfArray { Count: > 0 } arr => IsAsciiFilter(arr.RawItems[0]),
        _ => false
    };

    private static bool TryGetLength(DictionaryBuilder dict, out int length)
    {
        if (dict.TryGetValue(KnownNames.Length, out var item)
            && item is PdfNumber num)
        {
            length = (int)num.IntValue;
            return true;
        }
        length = 0;
        return false;
    }
}