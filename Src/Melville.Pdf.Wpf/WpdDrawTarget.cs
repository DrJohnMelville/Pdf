using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public class WpfDrawTarget : IDrawTarget
{
    private readonly DrawingContext context;
    private readonly GraphicsStateStack state;
    private readonly PathGeometry geometry = new();
    private PathFigure? figure = null;

    public WpfDrawTarget(DrawingContext context, GraphicsStateStack state)
    {
        this.context = context;
        this.state = state;
    }

    public void MoveTo(double x, double y)
    {
        figure = new PathFigure(){StartPoint = new Point(x, y)};
        geometry.Figures.Add(figure);
    }

    public void LineTo(double x, double y) => figure?.Segments.Add(new LineSegment(new Point(x,y), true));

    public void CurveTo(
        double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY) => 
        figure?.Segments.Add(new BezierSegment(
            new Point(control1X, control1Y), new Point(control2X, control2Y), new Point(finalX, finalY), true));

    public void ClosePath()
    {
        if (figure != null) figure.IsClosed = true;
    }

    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        SetCurrentFillRule(evenOddFillRule);
        InnerPathPaint(stroke, fill, geometry);
    }

    private void InnerPathPaint(bool stroke, bool fill, Geometry pathToPaint) =>
        context.DrawGeometry(
            fill ? state.Current().Brush() : null, 
            stroke ? state.Current().Pen() : null, 
            pathToPaint);

    private void SetCurrentFillRule(bool evenOddFillRule) => 
    geometry.FillRule = evenOddFillRule ? FillRule.EvenOdd : FillRule.Nonzero;

    public void ClipToPath(bool evenOddRule)
    {
        SetCurrentFillRule(evenOddRule);
        context.PushClip(geometry);
    }
}