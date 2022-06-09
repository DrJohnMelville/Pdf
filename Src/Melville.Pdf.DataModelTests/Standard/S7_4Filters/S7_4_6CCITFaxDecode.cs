using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
// the next using is used by the macros
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters;

public ref struct CCITEncodingTestBuilder
{
    private CcittBitWriter writer = new(new byte[100], new BitWriter());

    public CCITEncodingTestBuilder()
    {
    }

    public CCITEncodingTestBuilder P()
    {
        writer.WritePass();
        return this;
    }

    public CCITEncodingTestBuilder VL(int delta) => V(-delta);
    public CCITEncodingTestBuilder VR(int delta) => V(delta);
    public CCITEncodingTestBuilder V(int delta)
    {
        writer.WriteVertical(delta);
        return this;
    }

    public CCITEncodingTestBuilder HW(int first, int second) => H(true, first, second);
    public CCITEncodingTestBuilder HB(int first, int second) => H(false, first, second);
    public CCITEncodingTestBuilder H(bool whiteFirst, int first, int second)
    {
        writer.WriteHorizontal(whiteFirst, first, second);
        return this;
    }

    public string Build() => ExtendedAsciiEncoding.ExtendedAsciiString(writer.WrittenSpan());
}

[MacroItem("\xff\xff\xff\xff","V(0)", "AllWhite", "JustData32v4")]
[MacroItem("\xff\x00\x00\xff","HW(8,16).V(0)", "WhiteBlackWhite", "JustData32v4")]
[MacroItem("\x00\xFF\xFF\x00","HW(0,8).HW(16,8)", "BackWhiteBlack", "JustData32v4")]
[MacroItem("\x00\xFF\xFF\x0F\x00\x00\x00\x0F","HW(0,8).HW(16,4).V(0).V(0).P().V(0).V(0)", "InitialPass", "JustData32v4")]
[MacroItem("\x0\x0\x0\x0","HW(0,32)", "AllBlack", "JustData32v4")]
[MacroItem("\xFF\xFF\xFF\xF8","VL(3).V(0)", "VerticalMinusThree", "JustData32v4")]
[MacroItem("\xFF\xFF\xFF\xFE","VL(1).V(0)", "VerticalMinusOne", "JustData32v4")]
[MacroItem("\x55\x55\x55\x55", "HW(0,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).HW(1,1).VL(2).VL(1).V(0)", "Alternate1And0", "JustData32v4")]
[MacroItem("\x0\x0\x0\x0\x0\x0\x0\x0","HW(0,32).V(0).V(0)", "TwoBlack", "JustData32v4")]
[MacroCode("public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",new CCITEncodingTestBuilder().~1~.Build(), KnownNames.CCITTFaxDecode, S7_4_6CCITFaxDecode.~3~){}}")]
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