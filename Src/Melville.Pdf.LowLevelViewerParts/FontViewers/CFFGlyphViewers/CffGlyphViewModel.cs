using System.Net.Sockets;
using System.Numerics;
using System.Text;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

public partial class CffGlyphViewModel
{
    [FromConstructor] private readonly CffGlyphSource GlpyhSource;
    [AutoNotify] private CffGlyphBuffer? renderedGlyph;
    [AutoNotify] private bool unitSquare = true;
    [AutoNotify] private bool boundingBox = true;
    [AutoNotify] private bool points = true;
    [AutoNotify] private bool controlPoints = true;
    [AutoNotify] private bool phantomPoints = true;
    [AutoNotify] private bool outline = true;
    [AutoNotify] private bool fill = false;
    public PageSelectorViewModel PageSelector { get; } = new();

    partial void OnConstructed()
    {
        PageSelector.MinPage = 0;
        PageSelector.MaxPage = GlpyhSource.GlyphCount-1;
        PageSelector.WhenMemberChanges(nameof(PageSelector.Page), LoadNewGlyph);
        LoadNewGlyph();
    }

    private async void LoadNewGlyph()
    {
        var renderTemp = new CffGlyphBuffer();
        await GlpyhSource.RenderCffGlyphAsync((uint)PageSelector.Page, renderTemp, Matrix3x2.Identity);
        RenderedGlyph = renderTemp;
    }
}

public class CffGlyphBuffer : ICffGlyphTarget
{
    public  List<ICffAction> Output { get; } = new();

    public void Operator(CharStringOperators opCode, Span<DictValue> stack)
    {
        AddInstr(new CffOperator(opCode, stack));
    }

    private void AddInstr(ICffAction action) => Output.Add(action);

    public void RelativeCharWidth(float delta) => 
        AddInstr(new CffCharWidthAction(delta));

    public void MoveTo(Vector2 point) => AddInstr(new CffMoveToAction(point));
    public void LineTo(Vector2 point) => AddInstr(new CffLineToAction(point));
    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint) => 
        AddInstr(new CffCurveToAction(control1, control2, endPoint));
    // CFF fonts never call this, but it is required for the more generic
    // interface.
    public void CurveTo(Vector2 control, Vector2 endPoint) =>
        CurveTo(control, control, endPoint);
    public void EndGlyph() => AddInstr(new CffEndGlyphAction());
}

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