using System.Windows.Media;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer;

internal ref struct SplineDrawer
{
    CapturedPoint lastPoint;
    private bool lastOnCurve = true;
    PathFigure figure;
    private readonly ScaledDrawContext dc;
    private readonly CapturedPoint initialPoint;

    public SplineDrawer(ScaledDrawContext dc, CapturedPoint initialPoint)
    {
        this.dc = dc;
        this.initialPoint = initialPoint;
        lastPoint = initialPoint;
        figure = dc.MoveTo(initialPoint.Point.X, initialPoint.Point.Y);
    }

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

        lastPoint = point;
        lastOnCurve = true;
    }

    private void OffCurvePoint(in CapturedPoint point)
    {
        if (!lastOnCurve)
        {
            var midpoint = 
                new CapturedPoint((point.Point + lastPoint.Point)/2, CapturedPointFlags.OnCurve);
            DrawSplineTo(midpoint);
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