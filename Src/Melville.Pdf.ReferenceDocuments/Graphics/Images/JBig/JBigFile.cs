using Melville.Hacks;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;

public class JBig042_1: JBigFile { public JBig042_1() : base("042_1", 1728, 2339) { } }

public abstract class JBigFile : JBigBitStream
{
    private readonly string file;

    protected JBigFile(string file, int width, int height) : 
        base($"JBIG reference stream {file}", 1, width, height)
    {
        this.file = file;
    }

    protected override byte[] SourceBits()
    {
        var stream = GetType().Assembly.GetManifestResourceStream(
            $"Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig.{file}.jb2");
        var buffer = new byte[stream.Length];
        buffer.FillBuffer(0, buffer.Length, stream.Read);
        return buffer;
    }
}