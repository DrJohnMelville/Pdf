using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

internal static class ExponentialFunctionParser
{
    public static async ValueTask<PdfFunction> ParseAsync(PdfValueDictionary source)
    {
        var domain = await source.ReadIntervalsAsync(KnownNames.DomainTName).CA();
        var c0 = await source.ReadArrayWithDefaultAsync(KnownNames.C0TName, 0).CA();
        var c1 = await source.ReadArrayWithDefaultAsync(KnownNames.C1TName, 1).CA();
        if (domain.Length != 1) throw new PdfParseException("Type 2 functions must have a single input");
        if (c0.Length != c1.Length) throw new PdfParseException("C0 and C1 must have same number of elements");
        var transforms =  CreateExponentialTransforms(c0, c1);
        var n = await source.GetAsync<double>(KnownNames.NTName).CA();
        var range = await source.ReadOptionalRangesAsync(c1.Length).CA();
        if (transforms.Length != range.Length) 
            throw new PdfParseException("Must have a range for each function");
            
        return new ExponentialInterpolationFunction(domain, range, transforms, n);
    }

    private static ClosedInterval[] CreateExponentialTransforms(double[] c0, double[] c1) => 
        c0.Zip(c1, (i, j) => new ClosedInterval(i, j)).ToArray();
        
}