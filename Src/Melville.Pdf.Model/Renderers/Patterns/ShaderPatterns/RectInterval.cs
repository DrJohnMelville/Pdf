﻿using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal readonly struct RectInterval
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

    public RectInterval Intersect(RectInterval other) =>
        new(Horizontal.Intersect(other.Horizontal), Vertical.Intersect(other.Vertical));
}