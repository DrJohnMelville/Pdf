using System.Numerics;
using System.Text;
using Melville.Pdf.Model.Renderers.FontRenderings;

namespace Melville.Pdf.TextExtractor;

internal class ConcatenatingTextTarget: IExtractedTextTarget
{
    private List<TextExtractionLine> lines = new();
    private TextExtractionLine? lineBuilder;

    public void BeginWrite(IRealizedFont font)
    {
    }

    public void EndWrite(in Matrix3x2 textMatrix)
    {
        lineBuilder?.EndWrite(ComputeLocation(textMatrix));
        lineBuilder = null;
    }

    public void WriteCharacter(char character, in Matrix3x2 textMatrix)
    {
        lineBuilder ??= FindPartialLine(ComputeLocation(textMatrix));
        lineBuilder.AppendCharacter(character);
    }

    private TextExtractionLine FindPartialLine(Vector2 startPoint) => 
        lines.FirstOrDefault(i=>i.IsPrefixTo(startPoint)) ?? NewExtractionLine();

    private TextExtractionLine NewExtractionLine()
    {
        var ret = new TextExtractionLine();
        lines.Add(ret);
        return ret;
    }

    private Vector2 ComputeLocation(in Matrix3x2 textMatrix) => 
        Vector2.Transform(new Vector2(), textMatrix);

    public string AllText() => string.Join(Environment.NewLine,
        lines.Select(i => i.RenderedText()));
}

internal class TextExtractionLine
{
    private readonly StringBuilder text = new StringBuilder();
    public Vector2 EndPoint { get; set; }

    public void AppendCharacter(char character) => text.Append(character);

    public void EndWrite(Vector2 endPosition) => EndPoint = endPosition;

    public string RenderedText() => text.ToString();

    public bool IsPrefixTo(in Vector2 nextPoint) =>
        (EndPoint - nextPoint).LengthSquared() < 1.0;
}