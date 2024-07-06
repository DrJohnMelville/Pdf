using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using System.Numerics;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

public interface ICffAction
{
    void Execute(ICffGlyphTarget target);
}

public class CffOperator: ICffAction
{
    private string result;

    public CffOperator(CharStringOperators opCode, Span<DictValue> stack)
    {
        result = $"## {opCode}({string.Join(", ", stack.ToArray().Select(i => i.ToString()))})";
    }

    public void Execute(ICffGlyphTarget target)
    {
    }

    public override string ToString() => result;
}

public class CffCharWidthAction : ICffAction
{
    private readonly float delta;

    public CffCharWidthAction(float delta) => this.delta = delta;

    public void Execute(ICffGlyphTarget target) => target.RelativeCharWidth(delta);

    public override string ToString() => $"CharWidth ({delta})";
}

public class CffMoveToAction : ICffAction
{
    private readonly Vector2 point;

    public CffMoveToAction(Vector2 point) => this.point = point;

    public void Execute(ICffGlyphTarget target) => target.MoveTo(point);

    public override string ToString() => $"MoveTo ({point})";
}

public class CffLineToAction(Vector2 point) : ICffAction
{
    public void Execute(ICffGlyphTarget target) => target.LineTo(point);

    public override string ToString() => $"LineTo ({point})";
}

public class CffCurveToAction(
    Vector2 control1, Vector2 control2, Vector2 endPoint) : ICffAction
{
    public void Execute(ICffGlyphTarget target) => target.CurveTo(control1, control2, endPoint);

    public override string ToString() => $"CurveTo ({control1}, {control2}, {endPoint})";
}

public class CffEndGlyphAction : ICffAction
{
    public void Execute(ICffGlyphTarget target) => target.EndGlyph();

    public override string ToString() => "End of Glyph";
}