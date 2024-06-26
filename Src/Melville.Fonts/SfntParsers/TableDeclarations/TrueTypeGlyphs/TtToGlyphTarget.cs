using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

internal class TtToGlyphTarget<T>(T target) :ITrueTypePointTarget where
    T: IGlyphTarget
{
    Vector2 lastPoint;
    private bool lastOnCurve = true;

    public void AddPoint(Vector2 point, bool onCurve, bool isContourStart, bool isContourEnd)
    {
        if (isContourStart)
            NewMethod(point);
        else if (onCurve)
            OnCurvePoint(point);
        else OffCurvePoint(point);
    }

    private void NewMethod(Vector2 point)
    {
        target.MoveTo(point);
        lastOnCurve = true;
        lastPoint = point;
    }

    private void OnCurvePoint(Vector2 point)
    {
        if (lastOnCurve)
        {
            target.LineTo(point);
        }
        else
        {
            target.CurveTo(lastPoint, point);
        }

        lastOnCurve = true;
        lastPoint = point;
    }


    private void OffCurvePoint(Vector2 point)
    {
        if (!lastOnCurve)
        {
            OnCurvePoint((point + lastPoint)/2);
        }

        lastOnCurve = false;
        lastPoint = point;
    }

    public void AddPhantomPoint(Vector2 point)
    {
    }
}