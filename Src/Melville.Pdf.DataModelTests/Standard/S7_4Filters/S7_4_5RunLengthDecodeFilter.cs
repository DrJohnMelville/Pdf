using System.Linq;
using Melville.INPC;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters;

[MacroItem("", "", "Empty")]
[MacroItem("xyz", "\x2xyz", "NoRepeats")]
[MacroItem("xy", "\x1xy", "NoRepeatsWithTwo")]
[MacroItem("x", "\x0x", "SingleCharCodesAsAReoead")]
[MacroItem("zzzzzzzzzz", "\xF7z", "LongRepeat")]
[MacroItem("zzzzzzzzzzxyz", "\xF7z\x2xyz", "ConcatRepeatToLiteral")]
[MacroItem("xyAzzzzzzzzzz", "\x2xyA\xF7z", "ConcatLiteralToRepeat")]
[MacroItem("xyzzzzzzzzzz", "\x1xy\xF7z", "ConcatT20Plus")]
[MacroItem("xzzzzzzzzzz", "\x0x\xF7z", "ConcatTwoRepeats")]
[MacroCode("public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\", KnownNames.RunLengthDecode){}}")]
public partial class S7_4_5RunLengthDecodeFilter
{
    public class LongRun : StreamTestBase
    {
        public LongRun(): base(string.Join("", Enumerable.Repeat('z', 138)), "\x81z\xf7z", 
            KnownNames.RunLengthDecode){}
    }
    public class LongRunUnique : StreamTestBase
    {
        public LongRunUnique(): base(string.Join("", Enumerable.Repeat("xy", 67)), 
            "\x7fxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxy\x05xyxyxy", 
            KnownNames.RunLengthDecode){}
    }
}