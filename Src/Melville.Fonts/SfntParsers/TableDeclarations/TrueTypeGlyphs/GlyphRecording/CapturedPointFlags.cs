namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

/// <summary>
/// Attributes of a point from a font outline
/// </summary>
[Flags]
public enum CapturedPointFlags : byte
{
    /// <summary>
    /// This point is on the curve, or not a control point
    /// </summary>
    OnCurve = 0x01,
    /// <summary>
    /// This is the starting point of a contourl
    /// </summary>
    Start = 0x02,
    /// <summary>
    /// This is the endpoint of a conture
    /// </summary>
    End = 0x04,
    /// <summary>
    /// This is a phantom point
    /// </summary>
    Phantom = 0x08,
}