using System;
using System.Numerics;

namespace Melville.Pdf.LowLevel.Model.Wrappers;

/// <summary>
/// A set of convenience Methods for working with Matrix3x2 objects.
/// </summary>
public static class Matrix3x2Operations
{
    /// <summary>
    /// Convenience method to transform a Vector2 with a Matrix3x2.
    /// </summary>
    /// <param name="matrix">The transformation matrix</param>
    /// <param name="vector">The input vector</param>
    /// <returns>A new vector with the transformed value.</returns>
    public static Vector2 Transform(this in Matrix3x2 matrix, in Vector2 vector) =>
        Vector2.Transform(vector, matrix);

    /// <summary>
    /// Convenience method to transform two floats into a transformed vector.
    /// </summary>
    /// <param name="matrix">The transformation matrix</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>A transformed vector.</returns>
    public static Vector2 Transform(this in Matrix3x2 matrix, float x, float y) =>
        matrix.Transform(new Vector2(x, y));

    /// <summary>
    /// Returs the smallest axix aligned rectangle that contains all of the points in the span.
    /// </summary>
    /// <param name="points">Points in the bounding box</param>
    /// <returns>Lower left and upper right points of the axis-aligned rectangle that
    /// contains the points</returns>
    public static (Vector2 ll, Vector2 ur) MinBoundingBox(this Span<Vector2> points)
    {
        var left = float.MaxValue;
        var right = float.MinValue;
        var bottom = float.MaxValue;
        var top = float.MinValue;

        foreach (var point in points)
        {
            left = Math.Min(left, point.X);
            right = Math.Max(right, point.X);
            bottom = Math.Min(bottom, point.Y);
            top = Math.Max(top, point.Y);
        }

        return (new Vector2(left, bottom), new Vector2(right, top));
    }
}