using System.Windows.Media;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer;

internal ref struct SplineDrawer(ScaledDrawContext dc, CapturedPoint initialPoint)
{
    CapturedPoint lastPoint = initialPoint;
    private CapturedPoint lastLinePoint = initialPoint;
    private bool lastOnCurve = true;
    PathFigure figure = dc.MoveTo(initialPoint.Point.X, initialPoint.Point.Y);

    public void AddPoint(in CapturedPoint point)
    {
        if (point.OnCurve)
            OnCurvePoint(point);
        else
            OffCurvePoint(point);
    }

    private void OnCurvePoint(in CapturedPoint point)
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

    private void OffCurvePoint(in CapturedPoint point)
    {
        if (!lastOnCurve)
        {
            var midpoint = 
                new CapturedPoint((point.Point + lastPoint.Point)/2, CapturedPointFlags.OnCurve);
            DrawSplineTo(midpoint);
            ;
            lastLinePoint = midpoint;
        }

        lastPoint = point;
        lastOnCurve = false;
    }

    private void DrawSplineTo(CapturedPoint point)
    {
        dc.ConicCurveTo(figure,
            lastPoint.Point.X, lastPoint.Point.Y,
            point.Point.X, point.Point.Y);
    }

    private void DrawLineTo(CapturedPoint point)
    {
        dc.LineTo(figure, point.Point.X, point.Point.Y);
    }

    public PathFigure CloseFigure()
    {
        AddPoint(initialPoint);
        return figure;
    }
}