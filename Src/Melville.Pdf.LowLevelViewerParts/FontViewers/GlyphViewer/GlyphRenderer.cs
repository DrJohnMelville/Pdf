using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer;

[MacroItem("GlyphRecorder", "Glyph")]
[MacroItem("bool", "UnitSquare")]
[MacroItem("bool", "BoundingBox")]
[MacroItem("bool", "Points")]
[MacroItem("bool", "ControlPoints")]
[MacroItem("bool", "PhantomPoints")]
[MacroItem("bool", "Outline")]
[MacroItem("bool", "Fill")]
[MacroCode("""
public static readonly DependencyProperty ~1~Property = 
    DependencyProperty.Register(
        "~1~", typeof(~0~), typeof(GlyphRenderer),
        new FrameworkPropertyMetadata(default(~0~), 
            FrameworkPropertyMetadataOptions.AffectsRender));
public ~0~ ~1~
{
    get => (~0~)GetValue(~1~Property);
    set => SetValue(~1~Property, value);
}

""")]
public partial class GlyphRenderer : FrameworkElement
{
    protected override void OnRender(DrawingContext drawingContext)
    {
        var matrix = Matrix3x2.CreateTranslation(0,-1)*
                     Matrix3x2.CreateScale(1,-1)*
                     Matrix3x2.CreateTranslation(-0.5f, -0.5f) *
                     Matrix3x2.CreateScale(ComputeZoomFactor()) *
                     Matrix3x2.CreateTranslation((float)ActualWidth / 2, (float)ActualHeight / 2);

        var scaled = new ScaledDrawContext(drawingContext, matrix);

        new GlyphDesignPainter(scaled, 
            UnitSquare ? new Pen(Brushes.LightGray, 1): null, 
            BoundingBox ? new Pen(Brushes.LightBlue, 1) : null,
            Points ? Brushes.Red: null,
            ControlPoints ? Brushes.Blue : null,
            PhantomPoints ? Brushes.Green : null,
            Outline ? new Pen(Brushes.Black, 1)  : null, 
            Fill ? Brushes.Black : null,
            Glyph).Paint();
    }

    private float ComputeZoomFactor() => (float)(Math.Min(ActualHeight, ActualWidth) * 0.8);
}

public readonly struct ScaledDrawContext(DrawingContext dc, Matrix3x2 transform)
{
    public void DrawRectangle(Brush? brush, Pen? pen, Rect rect)
    {
        var geometry = new GeometryGroup();
        geometry.Children.Add(TransformedLineGeometry(
            rect.Left, rect.Bottom, rect.Left, rect.Top));
        geometry.Children.Add(TransformedLineGeometry(
            rect.Left, rect.Top, rect.Right, rect.Top));
        geometry.Children.Add(TransformedLineGeometry(
            rect.Right, rect.Top, rect.Right, rect.Bottom));
        geometry.Children.Add(TransformedLineGeometry(
            rect.Right, rect.Bottom, rect.Left, rect.Bottom));
        dc.DrawGeometry(brush, pen, geometry);
    }

    private Geometry TransformedLineGeometry(double x1, double y1, double x2, double y2)
    {
        var p1 = TransformToPoint(x1, y1);
        var p2 = TransformToPoint(x2, y2);
        return new LineGeometry(p1, p2);
    }

    public Point TransformToPoint(double x, double y)
    {
        var final = TransfomToVector(x, y);
        return new Point(final.X, final.Y);
    }

    private Vector2 TransfomToVector(double x, double y) => 
        Vector2.Transform(new((float)x, (float)y), transform);

    public void DrawLine(Pen? bBoxPen, Vector2 p1, Vector2 p2)
    {
        p1 = Vector2.Transform(p1, transform);
        p2 = Vector2.Transform(p2, transform);
        dc.DrawLine(bBoxPen, new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
    }

    public void DrawPoint(Brush? pointBrush, double pointX, double pointY)
    {
        var center = TransfomToVector(pointX, pointY);
            dc.DrawEllipse(pointBrush, null, new Point(center.X, center.Y), 2.5, 2.5);
    }

    public PathFigure MoveTo(double x, double y)
    {
        var ret = new PathFigure();
        ret.StartPoint = TransformToPoint(x, y);
        return ret;
    }
    public void LineTo(PathFigure figure, double x, double y) => 
        figure.Segments.Add(new LineSegment(TransformToPoint(x, y), true));

    public void ConicCurveTo(PathFigure figure, double controlX, double controlY, double finalX, double finalY) =>
        figure.Segments.Add(new QuadraticBezierSegment(TransformToPoint(controlX, controlY), 
            TransformToPoint(finalX, finalY), true));

    public void Draw(PathGeometry figure, Pen? pen, Brush? brush)
    {
        dc.DrawGeometry(brush, pen, figure);
    }
}