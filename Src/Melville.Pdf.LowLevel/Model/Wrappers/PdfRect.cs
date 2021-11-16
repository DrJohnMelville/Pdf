using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers;

public readonly record struct PdfRect (double Left, double Bottom, double Right, double Top)
{
    public PdfArray ToPdfArray => new(
        new PdfDouble(Left), new PdfDouble(Bottom), new PdfDouble(Right), new PdfDouble(Top));

    public static async ValueTask<PdfRect> CreateAsync(PdfArray array)
    {
        if (array.Count != 4)
            throw new PdfParseException("Pdf Rectangle must have exactly 4 items.");
        var (left,right) = MinMax((PdfNumber)await array[0], (PdfNumber)await array[2]);
        var (bottom, top) = MinMax((PdfNumber)await array[1], (PdfNumber)await array[3]);
        return new PdfRect(left, bottom, right, top);
    }

    private static (PdfNumber min, PdfNumber max) MinMax(PdfNumber a, PdfNumber b) => 
        (a.DoubleValue > b.DoubleValue) ? (b, a) : (a, b);

    public double Width => Right - Left;
    public double Height => Top - Bottom;
}