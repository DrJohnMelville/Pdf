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
    public void MoveTo(Vector2 startPoint)
    {
    }

    /// <inheritdoc />
    public void LineTo(Vector2 endPoint)
    {
    }

    /// <inheritdoc />
    public void CurveTo(Vector2 control, Vector2 endPoint)
    {
    }

    /// <inheritdoc />
    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint)
    {
    }

    /// <inheritdoc />
    public void EndGlyph()
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