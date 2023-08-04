using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers;

/// <summary>
/// Represents the PDF rectangle object which is an axis aligned rectangle defined by two opposite
/// corners.  I regularize the coordinates in the parser such that Left &lt;= Right and
/// Bottom &lt;= Top.
/// </summary>
/// <param name="Left">The left edge of the rectangle.</param>
/// <param name="Bottom">The bottom edge of the rectangle.</param>
/// <param name="Right">The right edge of the rectangle.</param>
/// <param name="Top">The top edge of the rectangle.</param>
public readonly record struct PdfRect (double Left, double Bottom, double Right, double Top)
{
    /// <summary>
    /// Convert the rectangle to a PdfArray
    /// </summary>
    public PdfArray ToPdfArray => new(Left, Bottom, Right, Top);

    /// <summary>
    /// Parse from a PdfArray of at exactly 4 doubles.
    /// </summary>
    /// <param name="array">The CodeSource PDF array.</param>
    /// <returns>A  PdfRectangle.</returns>
    public static async ValueTask<PdfRect> CreateAsync(PdfArray array)
    {
        var nums = await array.CastAsync<double>().CA();
        return FromDoubleSpan(nums[0], nums[1], nums[2],nums[3]);
    }

    private static PdfRect FromDoubleSpan(double x1, double y1, double x2, double y2)
    {
        var (left, right) = MinMax(x1,x2);
        var (bottom, top) = MinMax(y1,y2);
        return new PdfRect(left, bottom, right, top);
    }

    private static (double min, double max) MinMax(double a, double b) => 
        (a > b) ? (b, a) : (a, b);

    /// <summary>
    /// The width of the rectangle.
    /// </summary>
    public double Width => Right - Left;
    /// <summary>
    /// The height of the rectangle.
    /// </summary>
    public double Height => Top - Bottom;

    /// <summary>
    /// Trnsform the lower left and upper right coordinates of this rectangle and return
    /// a rectangle defined by those points.  This is not the same as transforming the rectangle,
    /// which we cannot do because many affine transforms would not result in an axis aligned rectangle.
    /// </summary>
    /// <param name="transform">A matrix by which to transform</param>
    /// <returns>The transformed rectangle as defined above.</returns>
    public PdfRect Transform(in Matrix3x2 transform)
    {
        var ll = Vector2.Transform(new Vector2((float)Left, (float)Bottom), transform);
        var ur = Vector2.Transform(new Vector2((float)Right, (float)Top), transform);
        return FromDoubleSpan(ll.X, ll.Y, ur.X, ur.Y);
    }
}