using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

public readonly struct RectInterval
{
    public readonly ClosedInterval Horizontal { get; }
    public readonly ClosedInterval Vertical { get; }

    public RectInterval(ClosedInterval horizontal, ClosedInterval vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public bool OutOfRange(in Vector2 point) =>
        Horizontal.OutOfInterval(point.X) || Vertical.OutOfInterval(point.Y);
}