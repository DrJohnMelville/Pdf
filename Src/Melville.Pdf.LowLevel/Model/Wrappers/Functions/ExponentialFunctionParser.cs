using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions
{
    public static class ExponentialFunctionParser
    {
        public static async ValueTask<PdfFunction> Parse(PdfDictionary source)
        {
            var domain = await source.ReadIntervals(KnownNames.Domain);
            var c0 = await source.ReadArrayWithDefault(KnownNames.C0, 0);
            var c1 = await source.ReadArrayWithDefault(KnownNames.C1, 1);
            if (domain.Length != 1) throw new PdfParseException("Type 2 functions must have a single input");
            if (c0.Length != c1.Length) throw new PdfParseException("C0 and C1 must have same number of elements");
            var transforms =  CreateExponentialTransforms(c0, c1);
            var n = await source.GetAsync<PdfNumber>(KnownNames.N);
            var range = await source.ReadOptionalRanges(c1.Length);
            if (transforms.Length != range.Length) 
                throw new PdfParseException("Must have a range for each function");
            
            return new ExponentialInterpolationFunction(domain, range, transforms, n.DoubleValue);
        }

        private static ClosedInterval[] CreateExponentialTransforms(double[] c0, double[] c1) => 
            c0.Zip(c1, (i, j) => new ClosedInterval(i, j)).ToArray();
        
    }
}