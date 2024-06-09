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
        PageSelector.MaxPage = GlpyhSource.GlyphCount;
        PageSelector.WhenMemberChanges(nameof(PageSelector.Page), LoadNewGlyph);
        LoadNewGlyph();
    }

    private async void LoadNewGlyph()
    {
        var renderTemp = new CffGlyphBuffer();
        await GlpyhSource.RenderGlyph((uint)PageSelector.Page, renderTemp, Matrix3x2.Identity);
        RenderedGlyph = renderTemp;
    }
}

public class CffGlyphBuffer : ICffGlyphTarget
{
    public List<ICffAction> Output { get; } = new();

    public void RelativeCharWidth(float delta) => 
        Output.Add(new CffCharWidthAction(delta));

    public void MoveTo(Vector2 point) => Output.Add(new CffMoveToAction(point));
    public void LineTo(Vector2 point) => Output.Add(new CffLineToAction(point));
    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint) => 
        Output.Add(new CffCurveToAction(control1, control2, endPoint));
    public void EndGlyph() => Output.Add(new CffEndGlyphAction());
}

public interface ICffAction
{
    void Execute(ICffGlyphTarget target);
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

public class CffLineToAction : ICffAction
{
    private readonly Vector2 point;

    public CffLineToAction(Vector2 point) => this.point = point;

    public void Execute(ICffGlyphTarget target) => target.LineTo(point);

    public override string ToString() => $"LineTo ({point})";
}

public class CffCurveToAction : ICffAction
{
    private readonly Vector2 control1;
    private readonly Vector2 control2;
    private readonly Vector2 endPoint;

    public CffCurveToAction(Vector2 control1, Vector2 control2, Vector2 endPoint) => 
        (this.control1, this.control2, this.endPoint) = (control1, control2, endPoint);

    public void Execute(ICffGlyphTarget target) => target.CurveTo(control1, control2, endPoint);

    public override string ToString() => $"CurveTo ({control1}, {control2}, {endPoint})";
}

public class CffEndGlyphAction : ICffAction
{
    public void Execute(ICffGlyphTarget target) => target.EndGlyph();

    public override string ToString() => "End of Glyph";
}