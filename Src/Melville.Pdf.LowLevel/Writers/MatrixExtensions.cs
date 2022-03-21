using System;
using System.Buffers;
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
            new PdfDouble(matrix.M11),
            new PdfDouble(matrix.M12),
            new PdfDouble(matrix.M21),
            new PdfDouble(matrix.M22),
            new PdfDouble(matrix.M31),
            new PdfDouble(matrix.M32)
        );

    public static async ValueTask<Matrix3x2> AsMatrix3x2Async(this PdfArray array) =>
        new ReadOnlySpan<double>(await array.AsDoublesAsync().CA()).AsMatrix3x2();

    public static Matrix3x2 AsMatrix3x2(this in ReadOnlySpan<double> items) =>
        items.Length == 6
            ? new Matrix3x2((float)items[0], (float)items[1], (float)items[2], (float)items[3],
                (float)items[4], (float)items[5])
            : throw new PdfParseException("Matrix array must have exactly 6 elements");
}