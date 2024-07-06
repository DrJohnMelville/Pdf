using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public readonly struct GlyphPainter: ICffGlyphTarget
{
    public GlyphPainter()
    {
    }

    public void Operator(CharStringOperators opCode, Span<DictValue> stack)
    {
        
    }

    public void RelativeCharWidth(float delta)
    {
    }

    public PathGeometry Geometry { get; } = new PathGeometry() { FillRule = FillRule.Nonzero };

    public void MoveTo(Vector2 point)
    {
        Geometry.Figures.Add(
            new PathFigure() { StartPoint = new Point(point.X, point.Y) });
    }

    public void LineTo(Vector2 point) => 
        AddSegmentToCurrentPath(new LineSegment(ToPoint(point), true));

    private static Point ToPoint(Vector2 point) => new(point.X, point.Y);

    private void AddSegmentToCurrentPath(PathSegment lineSegment) => 
        Geometry.Figures[^1].Segments.Add(lineSegment);

    public void CurveTo(Vector2 control, Vector2 endPoint) =>
        AddSegmentToCurrentPath(new QuadraticBezierSegment(ToPoint(control), ToPoint(endPoint), true));

    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint) =>
        AddSegmentToCurrentPath(new BezierSegment(ToPoint(control1), ToPoint(control2), ToPoint(endPoint), true));

    public void EndGlyph()
    {
    }
}