using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Wpf.FontCaching;

namespace Melville.Pdf.Wpf.Rendering;

public class WpfPathCreator : IDrawTarget
{
    protected PathGeometry? Geometry { get; private set; }
    private PathFigure? figure = null;

    [MemberNotNull(nameof(Geometry))]
    public virtual PathGeometry RequireGeometry() => 
        Geometry ??= new PathGeometry();

    [MemberNotNull(nameof(Geometry))]
    protected void SetGeometry(PathGeometry SetGeometry)
    {
        Geometry = SetGeometry;
    }

    public void MoveTo(double x, double y)
    {
        figure = new PathFigure(){StartPoint = new Point(x, y)};
        RequireGeometry().Figures.Add(figure);
    }

    public void LineTo(double x, double y) => figure?.Segments.Add(new LineSegment(new Point(x,y), true));

    public void ConicCurveTo(double controlX, double controlY, double finalX, double finalY) =>
        figure?.Segments.Add(new QuadraticBezierSegment(new Point(controlX, controlY), new Point(finalX, finalY),
            true));

    public void CurveTo( 
        double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY) => 
        figure?.Segments.Add(new BezierSegment(
            new Point(control1X, control1Y), new Point(control2X, control2Y), new Point(finalX, finalY), true));

    public void ClosePath()
    {
        if (figure != null) figure.IsClosed = true;
    }

    public virtual void SetDrawingTransform(in Matrix3x2 transform)
    {
    }

    public virtual void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
    }

    public virtual void ClipToPath(bool evenOddRule)
    {
    }
}
public class WpfDrawTarget : WpfPathCreator
{
    private readonly DrawingContext context;
    private readonly GraphicsStateStack<WpfGraphicsState> state;
    private readonly OptionalContentCounter? optionalContentCounter;
    private readonly GeometryGroup geoGroup = new GeometryGroup();
  
    public WpfDrawTarget(DrawingContext context, GraphicsStateStack<WpfGraphicsState> state,
        OptionalContentCounter? optionalContentCounter)
    {
        this.context = context;
        this.state = state;
        this.optionalContentCounter = optionalContentCounter;
    }

    public override PathGeometry RequireGeometry()
    {
        if (Geometry is null) geoGroup.Children.Add(base.RequireGeometry());
        return Geometry;
    }

    public override void SetDrawingTransform(in Matrix3x2 transform)
    {
        SetGeometry(new PathGeometry() { Transform = transform.WpfTransform() });
        geoGroup.Children.Add(Geometry);
    }

    public void AddGeometry(in Matrix3x2 textMatrix, CachedGlyph cachedGlyph)
    {
        SetGeometry(cachedGlyph.CreateInstance(textMatrix));
        geoGroup.Children.Add(Geometry);
    }

    public override void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        SetCurrentFillRule(evenOddFillRule);
        if (optionalContentCounter?.IsHidden ?? false) return;
        InnerPathPaint(stroke, fill, geoGroup);
    }

    private void InnerPathPaint(bool stroke, bool fill, Geometry pathToPaint) =>
        context.DrawGeometry(
            fill ? state.Current().Brush() : null, 
            stroke ? state.Current().Pen() : null, 
            pathToPaint);

    private void SetCurrentFillRule(bool evenOddFillRule) =>
        geoGroup.FillRule = evenOddFillRule ? FillRule.EvenOdd : FillRule.Nonzero;

    public override void ClipToPath(bool evenOddRule)
    {
        SetCurrentFillRule(evenOddRule);
        context.PushClip(geoGroup);
    }
}