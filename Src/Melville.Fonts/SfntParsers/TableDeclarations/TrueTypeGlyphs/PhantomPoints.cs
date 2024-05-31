using System.Numerics;
using System.Runtime.CompilerServices;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

[InlineArray(4)] internal struct PhantomPoints {private Vector2 value;}

internal static class PhantomPointsImplementation {
    public static void Draw(this in PhantomPoints points, ITrueTypePointTarget target)
    {
        foreach (var point in points)
        {
            target.AddPhantomPoint(point);
        }
    }
}