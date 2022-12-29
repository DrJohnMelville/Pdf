using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers;

public static class MatrixExtensions
{
    public static PdfArray AsPdfArray(this in Matrix3x2 matrix) =>
        new(
            matrix.M11,
            matrix.M12,
            matrix.M21,
            matrix.M22,
            matrix.M31,
            matrix.M32
        );

    public static async ValueTask<Matrix3x2> AsMatrix3x2OrIdentityAsync(this PdfArray? array) =>
        array is { Count: 6 } ? await array.AsMatrix3x2Async().CA(): Matrix3x2.Identity;
    
    public static async ValueTask<Matrix3x2> AsMatrix3x2Async(this PdfArray array) =>
        new ReadOnlySpan<double>(await array.AsDoublesAsync().CA()).AsMatrix3x2();

    public static Matrix3x2 AsMatrix3x2(this in ReadOnlySpan<double> items) =>
        items.Length == 6
            ? new Matrix3x2((float)items[0], (float)items[1], (float)items[2], (float)items[3],
                (float)items[4], (float)items[5])
            : throw new PdfParseException("Matrix array must have exactly 6 elements");
}