using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
// the next using is used by the macros
using Melville.Pdf.DataModelTests.StreamUtilities;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters;

[MacroItem("\xff\xff\xff\xff","\x80", "AllWhite", "JustData32v4")]
[MacroItem("\xff\x00\x00\xff","3à", "WhiteBlackWhite", "JustData32v4")]
[MacroItem("\x0\x0\x0\x0","& Ô", "AllBlack", "JustData32v4")]
[MacroItem("\x0\x0\x0\x0\x0\x0\x0\x0","& Õ", "TwoBlack", "JustData32v4")]
[MacroCode("public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\", KnownNames.CCITTFaxDecode, S7_4_6CCITFaxDecode.~3~){}}")]
public partial class S7_4_6CCITFaxDecode
{
    public static PdfDictionary JustData32v4 => new DictionaryBuilder()
        .WithItem(KnownNames.K, -1)
        .WithItem(KnownNames.EncodedByteAlign, false)
        .WithItem(KnownNames.Columns, 32)
        .WithItem(KnownNames.EndOfBlock, false)
        .WithItem(KnownNames.BlackIs1, false)
        .WithItem(KnownNames.DamagedRowsBeforeError, 0)
        .AsDictionary();
}

