using System.Numerics;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers;

/// <summary>
/// This is a DrawTarget that does not draw anything.
/// </summary>
[StaticSingleton]
public partial class NullDrawTarget : IDrawTarget
{
    /// <inheritdoc />
    public void Dispose()
    {
    }

    /// <inheritdoc />
    public void SetDrawingTransform(in Matrix3x2 transform)
    {
    }

    /// <inheritdoc />
    public void MoveTo(double x, double y)
    {
    }

    /// <inheritdoc />
    public void LineTo(double x, double y)
    {
    }

    /// <inheritdoc />
    public void ConicCurveTo(double controlX, double controlY, double finalX, double finalY)
    {
    }

    /// <inheritdoc />
    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY)
    {
    }

    /// <inheritdoc />
    public void ClosePath()
    {
    }

    /// <inheritdoc />
    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
    }

    /// <inheritdoc />
    public void ClipToPath(bool evenOddRule)
    {
    }
}