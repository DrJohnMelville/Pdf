using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Pdf.Model.Renderers;

namespace Melville.Pdf.Wpf.Rendering;

internal class WpfPathCreator : IDrawTarget
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
        if (figure != null && ShouldClose(figure)) figure.IsClosed = true;
    }

    private bool ShouldClose(PathFigure pathFigure)
    {
        if (pathFigure.Segments.Count > 1 ||
            (pathFigure.Segments.Count == 1 && pathFigure.Segments[0] is not LineSegment)) return true;
        return false;
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

    public void Dispose()
    {
    }
}