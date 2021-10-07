using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers
{
    public readonly struct PdfRect
    {
        public PdfNumber Top { get; }
        public PdfNumber Bottom { get; }
        public PdfNumber Left { get; }
        public PdfNumber Right { get; }

        public PdfRect(PdfNumber top, PdfNumber bottom, PdfNumber left, PdfNumber right)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
        }

        public static async ValueTask<PdfRect> CreateAsync(PdfArray array)
        {
            if (array.Count != 4)
                throw new PdfParseException("Pdf Rectangle must have exactly 4 items.");
            var (left,right) = MinMax((PdfNumber)await array[0], (PdfNumber)await array[2]);
            var (bottom, top) = MinMax((PdfNumber)await array[1], (PdfNumber)await array[3]);
            return new PdfRect(top, bottom, left, right);
        }

        private static (PdfNumber min, PdfNumber max) MinMax(PdfNumber a, PdfNumber b) => 
            (a.DoubleValue > b.DoubleValue) ? (b, a) : (a, b);
    }
}