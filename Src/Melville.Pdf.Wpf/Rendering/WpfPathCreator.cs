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



    public void MoveTo(Vector2 startPoint)
    {
        figure = new PathFigure(){StartPoint = startPoint.AsPoint()};
        RequireGeometry().Figures.Add(figure);
    }

    public void LineTo(Vector2 endPoint) => figure?.Segments.Add(
        new LineSegment(endPoint.AsPoint(), true));

    public void CurveTo(Vector2 control, Vector2 endPoint) =>
        figure?.Segments.Add(new QuadraticBezierSegment(
            control.AsPoint(), endPoint.AsPoint(), true));

    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint) => 
        figure?.Segments.Add(new BezierSegment(
            control1.AsPoint(), control2.AsPoint(), endPoint.AsPoint(), true));

    public void ClosePath()
    {
        if (figure != null && ShouldClose(figure)) figure.IsClosed = true;
    }

    public void EndGlyph()
    {
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