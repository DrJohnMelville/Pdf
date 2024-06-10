using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;
using Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer;

[MacroItem("CffGlyphBuffer", "Glyph")]
[MacroItem("bool", "UnitSquare")]
[MacroItem("bool", "BoundingBox")]
[MacroItem("bool", "Points")]
[MacroItem("bool", "ControlPoints")]
[MacroItem("bool", "PhantomPoints")]
[MacroItem("bool", "Outline")]
[MacroItem("bool", "Fill")]
[MacroItem("int", "MaxIndex")]
[MacroCode("""
    public static readonly DependencyProperty ~1~Property =
        DependencyProperty.Register(
            "~1~", typeof(~0~), typeof(CffGlyphRenderer),
            new FrameworkPropertyMetadata(default(~0~),
                FrameworkPropertyMetadataOptions.AffectsRender));
    public ~0~ ~1~
    {
        get => (~0~)GetValue(~1~Property);
        set => SetValue(~1~Property, value);
    }

    """)]
public partial class CffGlyphRenderer: FrameworkElement
{
    protected override void OnRender(DrawingContext drawingContext)
    {
        var matrix = Matrix3x2.CreateTranslation(0,-1)*
                     Matrix3x2.CreateScale(1,-1)*
                     Matrix3x2.CreateTranslation(-0.5f, -0.5f) *
                     Matrix3x2.CreateScale(ComputeZoomFactor()) *
                     Matrix3x2.CreateTranslation((float)ActualWidth / 2, (float)ActualHeight / 2);

        var scaled = new ScaledDrawContext(drawingContext, matrix);

        new CffGlyphPainter(scaled, 
            UnitSquare ? new Pen(Brushes.LightGray, 1): null, 
            BoundingBox ? new Pen(Brushes.LightBlue, 1) : null,
            Points ? Brushes.Red: null,
            ControlPoints ? Brushes.Blue : null,
            PhantomPoints ? Brushes.Green : null,
            Outline ? new Pen(Brushes.Black, 1)  : null, 
            Fill ? Brushes.Black : null,
            Glyph).Paint(MaxIndex);
    }

    private float ComputeZoomFactor() => (float)(Math.Min(ActualHeight, ActualWidth) * 0.8);
}

public class CffGlyphPainter(
    ScaledDrawContext scaled, 
    Pen? unitPen, 
    Pen? bboxPen, 
    Brush? pointBrush, 
    Brush? controlPointBrush, 
    Brush? phantomPointBrush, 
    Pen? glyphPen, 
    Brush? glyphBrush, 
    CffGlyphBuffer glyph):ICffGlyphTarget
{

    private PathGeometry geometry = new PathGeometry(){FillRule = FillRule.Nonzero};
    private PathFigure? figure;

    public void Operator(CharStringOperators opCode, Span<DictValue> stack)
    {
    }

    public void Paint(int maxIndex)
    {
        scaled.DrawRectangle(null, bboxPen, new Rect(0,0, 1, 1));
        if (glyph is null) return;
        foreach (var action in glyph.Output.Take(
                     maxIndex >= 0?maxIndex+1:int.MaxValue))
        {
            action.Execute(this);
        }
        scaled.Draw(geometry, glyphPen, glyphBrush);
    }

    public void RelativeCharWidth(float delta)
    {
    }

    public void MoveTo(Vector2 point)
    {
        scaled.DrawPoint(pointBrush, point.X, point.Y);
        figure = scaled.MoveTo(point.X, point.Y);
        geometry.Figures.Add(figure);
    }

    public void LineTo(Vector2 point)
    {
        scaled.DrawPoint(pointBrush, point.X, point.Y);
        if (figure is null) return;
        scaled.LineTo(figure, point.X, point.Y);
    }

    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint)
    {
        scaled.DrawPoint(controlPointBrush, control1.X, control1.Y);
        scaled.DrawPoint(controlPointBrush, control2.X, control2.Y);
        scaled.DrawPoint(pointBrush, endPoint.X, endPoint.Y);
        if (figure is null) return;
        scaled.CubicCurveTo(figure,
            control1.X, control1.Y,
            control2.X, control2.Y,
            endPoint.X, endPoint.Y);
    }

    public void EndGlyph()
    {
    }
}