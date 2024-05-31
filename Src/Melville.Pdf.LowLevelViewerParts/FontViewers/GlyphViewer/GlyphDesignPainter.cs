using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.Linq;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer;

public class GlyphDesignPainter (
    ScaledDrawContext dc, 
    Pen? unitPen, 
    Pen? BBoxPen,
    Brush? pointBrush, 
    Brush? controlPointBrush, 
    Brush? phantomPointBrush, 
    Pen? glyphPen, 
    Brush? glyphBrush,
    GlyphRecorder points)
{
    public void Paint()
    {
        if (points.Count == 0) return;
        DrawGlyph();
        DrawUnitRect();
        DrawBoundingBox();
        DrawPoints();
    }

    private void DrawBoundingBox()
    {
        var (xMin, xMax) = points.Select(i=>i.Point.X).MinMax();
        var (yMin, yMax) = points.Select(i=>i.Point.Y).MinMax();
        dc.DrawRectangle(null, BBoxPen, new Rect(xMin, yMin, xMax - xMin, yMax - yMin));
    }

    private void DrawUnitRect() =>
        dc.DrawRectangle(null, unitPen, new Rect(0, 0, 1, 1));

    private void DrawPoints()
    {
        foreach (var point in points)
        {
            dc.DrawPoint(PickPointBrush(point), point.Point.X, point.Point.Y);
        }
    }

    private Brush? PickPointBrush(CapturedPoint point) => point switch
    {
        { IsPhantom: true } => phantomPointBrush,
        { OnCurve: true } => pointBrush,
        _ => controlPointBrush
    };


    private void DrawGlyph()
    {
        var drawer = new SplineDrawer(dc, points[0]);
        var geometry = new PathGeometry();
        foreach (var point in points)
        {
            if (point.IsPhantom) continue;
            if (point.Begin)
            {
                drawer = new SplineDrawer(dc, point);
            }
            else
            {
                drawer.AddPoint(point);
                if (point.End)
                {
                    geometry.Figures.Add(drawer.CloseFigure());
                }
            }
        }

        dc.Draw(geometry, glyphPen, glyphBrush);
    }

}