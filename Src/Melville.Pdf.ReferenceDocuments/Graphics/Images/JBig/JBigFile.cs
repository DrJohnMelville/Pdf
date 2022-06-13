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
// [MacroItem("042_12", 1728, 2339)]
// [MacroItem("042_13", 1728, 2339)]
// [MacroItem("042_14", 1728, 2339)]
// [MacroItem("042_15", 1728, 2339)]
// [MacroItem("042_16", 1728, 2339)]
// [MacroItem("042_17", 1728, 2339)]
// [MacroItem("042_18", 1728, 2339)]
// [MacroItem("042_19", 1728, 2339)]
// [MacroItem("042_20", 1728, 2339)]
[MacroItem("042_21", 1728, 2339)]
[MacroItem("042_22", 1728, 2339)]
[MacroItem("042_23", 1728, 2339)]
[MacroItem("042_24", 1728, 2339)]
[MacroItem("042_25", 1728, 2339)]
[MacroItem("amb_1", 800, 1200)]
[MacroItem("amb_2", 800, 1200)]
[MacroCode("public class JBig~0~: JBigFile { public JBig~0~() : base(\"~0~\", ~1~, ~2~) { } }")]
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