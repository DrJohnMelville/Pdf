using System;
using Melville.Icc.Model.Tags;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class LinearizedRgbToXyzMatrixFactory
{
    public static Matrix3x3 XyzFromSrgb(in FloatColor whitePoint) =>
        ToArbiitraryRgb(whitePoint, 0.64f, 0.33f, 0.30f, 0.60f, 0.15f, 0.06f);

    public static Matrix3x3 ToArbiitraryRgb(
        in FloatColor whitePoint, float xR, float yR, float xG, float yG, float xB, float yB)
    {
        var redCol = PartialCol(xR, yR);
        var greenCol = PartialCol(xG, yG);
        var blueCol = PartialCol(xB, yB);
        Span<float> adjustments = stackalloc float[] { whitePoint.Red, whitePoint.Green, whitePoint.Blue };

        new Matrix3x3(
            redCol.X, greenCol.X, blueCol.X,
            redCol.Y, greenCol.Y, blueCol.Y,
            redCol.Z, greenCol.Z, blueCol.Z
        ).Inverse().PostMultiplyBy(adjustments, adjustments);

        return new Matrix3x3(
            adjustments[0] * redCol.X, adjustments[1] * greenCol.X, adjustments[2] * blueCol.X,
            adjustments[0] * redCol.Y, adjustments[1] * greenCol.Y, adjustments[2] * blueCol.Y,
            adjustments[0] * redCol.Z, adjustments[1] * greenCol.Z, adjustments[2] * blueCol.Z
        );
    }
    private static FloatXyx PartialCol(float x, float y) =>
            new(x / y, 1, (1 - (x + y)) / y);
}

public record struct FloatXyx(float X, float Y, float Z);