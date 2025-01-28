using System.Numerics;
using System.Text;
using Melville.Pdf.Model.Renderers.FontRenderings;

namespace Melville.Pdf.TextExtractor;

internal class ConcatenateAndCollapseHyphenTarget : ConcatenatingTextTarget
{
    protected override TextExtractionLine? HyphenContinuation()
    {
        return lines.Count > 0 ? lines[^1].AsHyphenContinue() : null;
    }
}

internal class ConcatenatingTextTarget: IExtractedTextTarget
{
    protected List<TextExtractionLine> lines = new();
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
        var nextLocation = ComputeLocation(textMatrix);
        if (lineBuilder is null)
        {
            lineBuilder = FindPartialLine(nextLocation);
        }
        lineBuilder.AppendCharacter(character);
    }

    public void DeltaInsideWrite(double value)
    {
        if (value < -150 && lineBuilder is not null)
            lineBuilder.AppendCharacter(' ');
    }

    private TextExtractionLine FindPartialLine(Vector2 startPoint) => 
        HyphenContinuation() ??
        lines.FirstOrDefault(i=>i.IsPrefixTo(startPoint)) ?? 
        NewExtractionLine(startPoint);

    protected virtual TextExtractionLine? HyphenContinuation() => null;

    private TextExtractionLine NewExtractionLine(Vector2 startPoint)
    {
        var ret = new TextExtractionLine()
        {
            StartPoint = startPoint
        };
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
    public Vector2 StartPoint { get; set; }
    public Vector2 EndPoint { get; set; }

    public void AppendCharacter(char character) => text.Append(character);

    public void EndWrite(Vector2 endPosition) => EndPoint = endPosition;

    public string RenderedText() => text.ToString();

    public bool IsPrefixTo(in Vector2 nextPoint)
    {
        var delta = EndPoint - nextPoint;
        return (Math.Abs(delta.X) < 5f && Math.Abs(delta.Y) < 1f);
    }

    public TextExtractionLine? AsHyphenContinue()
    {
        if (text[^1] == '-')
        {
            text.Remove(text.Length - 1, 1);
            return this;
        }

        return null;
    }
}