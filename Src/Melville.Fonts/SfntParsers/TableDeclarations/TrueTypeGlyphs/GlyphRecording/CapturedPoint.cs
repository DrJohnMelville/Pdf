using System.Numerics;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

/// <summary>
/// This structure represents a single point extracted from a font outline
/// </summary>
public readonly partial struct CapturedPoint
{
    /// <summary>
    /// location of the point
    /// </summary>
    [FromConstructor] public Vector2 Point { get; }

    /// <summary>
    /// Attributes of this point
    /// </summary>
    [FromConstructor] public CapturedPointFlags Flags { get; }

    /// <summary>
    /// X component of the point
    /// </summary>
    public float X => Point.X;
    /// <summary>
    /// Y component of the point
    /// </summary>
    public float Y => Point.Y;
    /// <summary>
    /// Point is on the curve
    /// </summary>
    public bool OnCurve => (Flags & CapturedPointFlags.OnCurve) != 0;
    /// <summary>
    /// Point is the first point in a contour
    /// </summary>
    public bool Begin => (Flags & CapturedPointFlags.Start) != 0;
    /// <summary>
    /// Point is the last point in a contour
    /// </summary>
    public bool End => (Flags & CapturedPointFlags.End) != 0;
    /// <summary>
    /// Point is the last point in a contour
    /// </summary>
    public bool IsPhantom => (Flags & CapturedPointFlags.Phantom) != 0;

}