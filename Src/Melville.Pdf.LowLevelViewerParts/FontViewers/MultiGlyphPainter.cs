using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;
using Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

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

public partial class MultiGlyphPainter: FrameworkElement
{
    [GenerateDP]
    public static readonly DependencyProperty GlyphSourceProperty = DependencyProperty.Register(
        "GlyphSource", typeof(IGlyphSource), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(default(IGlyphSource),
            FrameworkPropertyMetadataOptions.AffectsArrange));
    
    [GenerateDP]
    public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
        "Rows", typeof(int), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));
    
    [GenerateDP]
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
        "Columns", typeof(int), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));
     
    [GenerateDP]
    public static readonly DependencyProperty PageProperty = DependencyProperty.Register(
        "Page", typeof(int), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    [GenerateDP]
    public static readonly DependencyProperty GlyphSizeProperty = DependencyProperty.Register(
        "GlyphSize", typeof(double), typeof(MultiGlyphPainter), 
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    protected override void OnRender(DrawingContext drawingContext)
    {
        var defaultMatrix = Matrix3x2.CreateTranslation(-0.5f,-0.5f)*Matrix3x2.CreateScale((float)GlyphSize,(float)-GlyphSize);
        var deltaX = (float) ActualWidth / Columns;
        var deltaY = (float) ActualHeight / Rows;
        int first = (Page - 1) * Rows * Columns;
        for (int i = 0; i < Rows; i++)
        for (int j = 0; j < Columns; j++)
        {
            if (first >= GlyphSource.GlyphCount) return;
            DrawGlyph(drawingContext, first, defaultMatrix*Matrix3x2.CreateTranslation(deltaX*(j+0.5f), deltaY * (i+0.5f)));

            first++;
        }
    }

    private void DrawGlyph(DrawingContext drawingContext, int first, Matrix3x2 targetMatrix)
    {
        var glyph = new CffGlyphBuffer();
        var source = GlyphSource; // capture to source while on the UI thread.
        Task.Run(async () =>
        {
            await source.RenderGlyphAsync((uint)first, glyph, targetMatrix);
        }).GetAwaiter().GetResult();
        var painter = new GlyphPainter();
        foreach (var action in glyph.Output)
        {
            action.Execute(painter);
        }
        drawingContext.DrawGeometry(Brushes.Black, null, painter.Geometry);
    }
}