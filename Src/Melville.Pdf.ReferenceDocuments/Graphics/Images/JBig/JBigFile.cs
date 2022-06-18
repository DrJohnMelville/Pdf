using Melville.Hacks;
using Melville.INPC;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;

[MacroItem("042_1", 1728, 2339)]
[MacroItem("042_2", 1728, 2339)]
[MacroItem("042_3", 1728, 2339)]
[MacroItem("042_4", 1728, 2339)]
[MacroItem("042_5", 1728, 2339)]
[MacroItem("042_6", 1728, 2339)]
[MacroItem("042_7", 1728, 2339)]
[MacroItem("042_8", 1728, 2339)]
[MacroItem("042_9", 1728, 2339)]
[MacroItem("042_10", 1728, 2339)]
[MacroItem("042_11", 1728, 2339)]
[MacroItem("042_12", 1728, 2339)]
//\[MacroItem("042_13", 1728, 2339)]
//[MacroItem("042_14", 1728, 2339)]
[MacroItem("042_15", 1728, 2339)]
[MacroItem("042_16", 1728, 2339)]
[MacroItem("042_17", 1728, 2339)]
[MacroItem("042_18", 1728, 2339)]
[MacroItem("042_19", 1728, 2339)]
[MacroItem("042_20", 1728, 2339)]
[MacroItem("042_21", 1728, 2339)]
[MacroItem("042_22", 1728, 2339)]
[MacroItem("042_23", 1728, 2339)]
[MacroItem("042_24", 1728, 2339)]
[MacroItem("042_25", 1728, 2339)]
[MacroItem("amb_1", 800, 1200)]
[MacroItem("amb_2", 800, 1200)]
[MacroItem("200__10__0", 1700,2200)]
[MacroItem("200__10__0__stripe", 1700,2200)]
[MacroItem("200__10__45", 1700,2200)]
[MacroItem("200__10__45__stripe", 1700,2200)]
[MacroItem("200__2__0", 1700,2200)]
[MacroItem("200__2__0__stripe", 1700,2200)]
[MacroCode("public class JBig~0~: JBigFile { public JBig~0~() : base(\"~0~\".Replace(\"__\",\"-\"), ~1~, ~2~) { } }")]
[MacroItem("200__20__0", 1700,2200)]
[MacroItem("200__20__0__stripe", 1700,2200)]
[MacroItem("200__20__45", 1700,2200)]
[MacroItem("200__20__45__stripe", 1700,2200)]
[MacroItem("200__3__0", 1700,2200)]
[MacroItem("200__3__0__stripe", 1700,2200)]
[MacroItem("200__3__45", 1700,2200)]
[MacroItem("200__3__45__stripe", 1700,2200)]
[MacroItem("200__4__0", 1700,2200)]
[MacroItem("200__4__0__stripe", 1700,2200)]
[MacroItem("200__4__45", 1700,2200)]
[MacroItem("200__4__45__stripe", 1700,2200)]
[MacroItem("200__5__0", 1700,2200)]
[MacroItem("200__5__0__stripe", 1700,2200)]
[MacroItem("200__5__45", 1700,2200)]
[MacroItem("200__5__45__stripe", 1700,2200)]
[MacroItem("200__6__0", 1700,2200)]
[MacroItem("200__6__0__stripe", 1700,2200)]
[MacroItem("200__6__45", 1700,2200)]
[MacroItem("200__6__45__stripe", 1700,2200)]
[MacroItem("200__8__0", 1700,2200)]
[MacroItem("200__8__0__stripe", 1700,2200)]
[MacroItem("200__8__45", 1700,2200)]
[MacroItem("200__8__45__stripe", 1700,2200)]
[MacroItem("200__lossless", 1700,2200)]
// [MacroItem("600__10__0", 1700,2200)]
// [MacroItem("600__6__0", 1700,2200)]
// [MacroItem("600__6__45", 1700,2200)]
public abstract partial class JBigFile : JBigBitStream
{
    private readonly string file;

    protected JBigFile(string file, int width, int height) : 
        base($"JBIG reference stream {file}", 1, width, height)
    {
        this.file = file;
    }

    protected override byte[] SourceBits()
    {
        var stream = GetStream(file)!;
        var buffer = new byte[stream.Length];
        buffer.FillBuffer(0, buffer.Length, stream.Read);
        return buffer;
    }

    private Stream? GetStream(string name) => JBigSampleStreams.Get(name);
}