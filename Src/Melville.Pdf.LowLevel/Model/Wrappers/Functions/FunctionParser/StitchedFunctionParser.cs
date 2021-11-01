using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

public static class StitchedFunctionParser
{
    public static async ValueTask<PdfFunction> Parse(PdfDictionary source)
    {
        var domain = await source.ReadIntervals(KnownNames.Domain);
        var encode = await source.ReadIntervals(KnownNames.Encode);
        var bounds = await (await source.GetAsync<PdfArray>(KnownNames.Bounds)).AsDoublesAsync();
        var functionDecls =
            await (await source.GetAsync<PdfArray>(KnownNames.Functions)).AsAsync<PdfDictionary>();
        var functions = await CreateFunctionSegments(functionDecls, domain[0], bounds, encode);

        var range = await source.ReadOptionalRanges(functions[0].NumberOfOutputs);
        return new StitchedFunction(domain, range, functions);
    }
        
    private static async Task<StitchedFunctionSegment[]> CreateFunctionSegments(
        PdfDictionary[] functionDecls, ClosedInterval domain, double[] bounds, 
        ClosedInterval[] encode)
    {
        var functions = new StitchedFunctionSegment[functionDecls.Length];
        for (int i = 0; i < functionDecls.Length; i++)
        {
            functions[i] = new StitchedFunctionSegment(
                SegmentDomain(bounds, i, domain),
                encode[i],
                await functionDecls[i].CreateFunction());
        }

        return functions;
    }

    private static ClosedInterval SegmentDomain(double[] bounds, int i, ClosedInterval domain) =>
        new(
            i == 0 ? domain.MinValue : bounds[i-1],
            i >= bounds.Length ? domain.MaxValue : bounds[i]);
}