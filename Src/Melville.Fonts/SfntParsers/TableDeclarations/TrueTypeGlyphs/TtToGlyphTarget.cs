using System.Numerics;
using Melville.Parsing.ObjectRentals;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

internal class TtToGlyphTarget<T> :ITrueTypePointTarget, IClearable where
    T: IGlyphTarget
{
    private T target = default(T)!;

    public TtToGlyphTarget<T> WithTarget(T target)
    {
        this.target = target;
        return this;
    }

    public void Clear()
    {
        target = default(T)!;
    }

    Vector2 lastPoint;
    private bool lastOnCurve = true;
    Vector2 contourStartPoint = Vector2.Zero;
    private bool contourStartOnCurve = false;

    public void AddPoint(Vector2 point, bool onCurve, bool isContourStart, bool isContourEnd)
    {
        if (isContourStart)
            ContourStartPoint(point, onCurve);
        else
            InsertPoint(point, onCurve);
        if (isContourEnd)
            CloseContour();
    }

    private void InsertPoint(Vector2 point, bool onCurve)
    {
        if (onCurve)
            OnCurvePoint(point);
        else OffCurvePoint(point);
    }

    private void CloseContour() => InsertPoint(contourStartPoint, contourStartOnCurve);

    private void ContourStartPoint(Vector2 point, bool isOnCurve)
    {
        target.MoveTo(point);
        contourStartPoint = point;
        contourStartOnCurve = isOnCurve;
        lastOnCurve = isOnCurve;
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