using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers;

/// <summary>
/// This class is a collection of convenience methods for transforming Melville.Pdf's preferred matrix representation
/// the Matrix32 vlass from System.Numerics to and from various pdf representations
/// </summary>
public static class MatrixExtensions
{
    /// <summary>
    /// Convert a Matrix32x to a pdf array of 6 numbers
    /// </summary>
    /// <param name="matrix">The matrix to convert</param>
    /// <returns>The resulting PDFArray</returns>
    public static PdfValueArray AsPdfArray(this in Matrix3x2 matrix) =>
        new(
            matrix.M11,
            matrix.M12,
            matrix.M21,
            matrix.M22,
            matrix.M31,
            matrix.M32
        );

    /// <summary>
    /// Convert a PdfArray into a Matrix3x2, or The identity matrix if the array is the wrong length
    /// </summary>
    /// <param name="array">The array to convert</param>
    /// <returns>The resulting matrix</returns>
    public static async ValueTask<Matrix3x2> AsMatrix3x2OrIdentityAsync(this PdfValueArray? array) =>
        array is { Count: 6 } ? await array.AsMatrix3x2Async().CA(): Matrix3x2.Identity;

    /// <summary>
    /// Convert a PdfArray into a Matrix3x2
    /// </summary>
    /// <param name="array">The array to convert</param>
    /// <returns>The resulting matrix</returns>
    /// <exception cref="PdfParseException">If the PdfArray does not contain exactly 6 doubles.</exception>
    public static async ValueTask<Matrix3x2> AsMatrix3x2Async(this PdfValueArray array) =>
        new ReadOnlySpan<double>(await array.CastAsync<double>().CA()).AsMatrix3x2();

    /// <summary>
    /// Convert a span of doubles into a Matrix3x2
    /// </summary>
    /// <param name="items">a span which must contain exactly 6 doubles</param>
    /// <returns>The resulting Matrix3x2</returns>
    /// <exception cref="PdfParseException">If the span does not contain exaclty 6 doubles.</exception>
    public static Matrix3x2 AsMatrix3x2(this in ReadOnlySpan<double> items) =>
        items.Length == 6
            ? new Matrix3x2((float)items[0], (float)items[1], (float)items[2], (float)items[3],
                (float)items[4], (float)items[5])
            : throw new PdfParseException("Matrix array must have exactly 6 elements");
}