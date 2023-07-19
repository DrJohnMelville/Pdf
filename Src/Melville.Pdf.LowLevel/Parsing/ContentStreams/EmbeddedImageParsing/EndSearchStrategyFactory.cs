using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal static class EndSearchStrategyFactory
{
    public static EndSearchStrategy Create(ValueDictionaryBuilder dict)
    {
        if (!TryGetLength(dict, out var length)) return EndSearchStrategy.Instance;
        if (dict.TryGetValue(KnownNames.FilterTName, out var arrItem) && IsAsciiFilter(arrItem.Value))
            return new WhiteSpaceAndLengthSearchStrategy(length);
        else
            return new WithLengthSearchStrategy(length);
    }

    private static bool IsAsciiFilter(PdfIndirectValue arrItem) => 
        arrItem.TryGetEmbeddedDirectValue(out var dirval) && dirval switch
    {
        _ when dirval.Equals(KnownNames.A85TName) ||
               dirval.Equals(KnownNames.AHxTName) ||
               dirval.Equals(KnownNames.ASCII85DecodeTName) ||
               dirval.Equals(KnownNames.ASCIIHexDecodeTName) => true,
        _ when dirval.TryGet(out PdfValueArray? arr) && arr.Count > 0 => 
            IsAsciiFilter(arr.RawItems[0]),
        _ => false
    };

    private static bool TryGetLength(ValueDictionaryBuilder dict, out int length) =>
        dict.TryGetValue(KnownNames.LengthTName, out var item) &&
        item.Value.TryGetEmbeddedDirectValue(out var dirLength) &&
        dirLength.TryGet(out length) ||
        0.AsFalseValue(out length);
}