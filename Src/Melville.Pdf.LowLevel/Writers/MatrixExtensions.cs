using System.Numerics;
using Melville.Pdf.LowLevel.Model.Objects;

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
}