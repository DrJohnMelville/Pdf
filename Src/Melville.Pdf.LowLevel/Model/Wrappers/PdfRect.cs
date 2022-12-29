using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers;

public readonly record struct PdfRect (double Left, double Bottom, double Right, double Top)
{
    public PdfArray ToPdfArray => new(Left, Bottom, Right, Top);

    public static async ValueTask<PdfRect> CreateAsync(PdfArray array)
    {
        var nums = await array.AsDoublesAsync().CA();
        return FromDoubleSpan(nums);
    }

    private static PdfRect FromDoubleSpan(in ReadOnlySpan<double> nums)
    {
        if (nums.Length != 4)
            throw new PdfParseException("Pdf Rectangle must have exactly 4 items.");
        var (left, right) = MinMax(nums[0], nums[2]);
        var (bottom, top) = MinMax(nums[1], nums[3]);
        return new PdfRect(left, bottom, right, top);
    }

    private static (double min, double max) MinMax(double a, double b) => 
        (a > b) ? (b, a) : (a, b);

    public double Width => Right - Left;
    public double Height => Top - Bottom;

    public PdfRect Transform(in Matrix3x2 transform)
    {
        var ll = Vector2.Transform(new Vector2((float)Left, (float)Bottom), transform);
        var ur = Vector2.Transform(new Vector2((float)Right, (float)Top), transform);
        Span<double> nums = stackalloc double[] { ll.X, ll.Y, ur.X, ur.Y };
        return FromDoubleSpan(nums);
    }
}