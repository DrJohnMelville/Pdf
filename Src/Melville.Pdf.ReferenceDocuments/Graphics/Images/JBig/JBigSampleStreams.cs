namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;

public static class JBigSampleStreams
{
    public static Stream? Get(string name)
    {
        return typeof(JBigSampleStreams).Assembly.GetManifestResourceStream(
            $"Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig.{name}.jb2");
    }
}