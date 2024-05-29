using System.Numerics;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer
{
    public class RecordedGlyph : ITrueTypePointTarget
    {

        private readonly List<GlyphPoint> points = new();
        private readonly List<BoundingBox> boxes = new();

        public void BeginGlyph(
            int level, short minX, short minY, short maxX, short maxY, in Matrix3x2 transform) => 
            boxes.Add(new(minX, minY, maxX, maxY, level, transform));

        public void AddPoint(double x, double y, bool onCurve, bool isContourStart, bool isContourEnd)
        {
            points.Add(new(x, y, isContourStart, isContourEnd, onCurve));
        }

        public void EndGlyph(int level)
        {
        }

        private record struct GlyphPoint(double X, double Y, 
            bool Begin, bool End, bool OnCurve);

        private record struct BoundingBox(double x1, double y1, double x2, double y2, int level,
            Matrix3x2 Matrix)
        {
            public void Render(ScaledDrawContext dc, Pen? bBoxPen)
            {
                var p1 = TransformedPoint(x1, y1);
                var p2 = TransformedPoint(x2, y1);
                var p3 = TransformedPoint(x2, y2);
                var p4 = TransformedPoint(x1, y2);

                dc.DrawLine(bBoxPen, p1, p2);
                dc.DrawLine(bBoxPen, p2, p3);
                dc.DrawLine(bBoxPen, p3, p4);
                dc.DrawLine(bBoxPen, p4, p1);
            }

            private Vector2 TransformedPoint(double x, double y) => Vector2.Transform(new Vector2((float)x, (float)y), Matrix);
        }

        public void Paint(ScaledDrawContext dc, Pen? unitPen, Pen? BBoxPen,
            Brush? pointBrush, Pen? glyphPen, Brush? glyphBrush)
        {
            if (points.Count == 0) return;
            DrawGlyph(dc, glyphBrush, glyphPen, glyphBrush);
            DrawUnitRect(dc, unitPen);
            DrawBoundingRects(dc, BBoxPen);
            DrawPoints(dc, pointBrush);
        }

        private static void DrawUnitRect(ScaledDrawContext dc, Pen? unitPen) => dc.DrawRectangle(null, unitPen, new Rect(0,0,1,1));

        private void DrawBoundingRects(ScaledDrawContext dc, Pen? bBoxPen)
        {
            foreach (var box in boxes)
            {
                box.Render(dc, bBoxPen);
            }
        }
        private void DrawPoints(ScaledDrawContext dc, Brush? pointBrush)
        {
            foreach (var point in points)
            {
                dc.DrawPoint(pointBrush, point.X, point.Y, point.OnCurve);
            }
        }
        private void DrawGlyph(ScaledDrawContext dc, Brush? glyphBrush, Pen? glyphPen, Brush? brush)
        {
            var drawer = new SplineDrawer(dc, glyphPen, brush, points[0]);
            var geometry = new PathGeometry();
            foreach (var point in points)
            {
                if (point.Begin)
                {
                    drawer = new SplineDrawer(dc, glyphPen, brush, point);
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
            dc.Draw(geometry, glyphPen, brush);
        }

        private ref struct SplineDrawer(ScaledDrawContext dc, Pen? pen, Brush? brush, GlyphPoint initialPoint)
        {
            GlyphPoint lastPoint = initialPoint;
            private GlyphPoint lastLinePoint = initialPoint;
            private bool lastOnCurve = true;
            PathFigure figure = dc.MoveTo(initialPoint.X, initialPoint.Y);

            public void AddPoint(GlyphPoint point)
            {
                if (point.OnCurve)
                    OnCurvePoint(point);
                else
                    OffCurvePoint(point);
            }

            private void OnCurvePoint(GlyphPoint point)
            {
                if (lastOnCurve)
                {
                    DrawLineTo(point);
                }
                else
                {
                    DrawSplineTo(point);
                }
                lastLinePoint = lastPoint = point;
                lastOnCurve = true;
            }

            private void OffCurvePoint(GlyphPoint point)
            {
                if (!lastOnCurve)
                {
                    var midpoint = new GlyphPoint(
                        (lastPoint.X + point.X) / 2, (lastPoint.Y + point.Y) / 2,
                        false, false, true);
                    DrawSplineTo(midpoint); ;
                    lastLinePoint = midpoint;
                }
                lastPoint = point;
                lastOnCurve = false;
            }

            private void DrawSplineTo(GlyphPoint point)
            {
                dc.ConicCurveTo(figure,
                    lastPoint.X, lastPoint.Y, 
                    point.X, point.Y);
            }

            private void DrawLineTo(GlyphPoint point)
            {
                dc.LineTo(figure, point.X, point.Y);
            }

            public PathFigure CloseFigure()
            {
                AddPoint(initialPoint);
                return figure;
            }
        }
    }
}