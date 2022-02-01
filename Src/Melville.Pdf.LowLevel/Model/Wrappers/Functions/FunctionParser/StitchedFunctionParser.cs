using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

public static class StitchedFunctionParser
{
    public static async ValueTask<PdfFunction> Parse(PdfDictionary source)
    {
        var domain = await source.ReadIntervals(KnownNames.Domain).CA();
        var encode = await source.ReadIntervals(KnownNames.Encode).CA();
        var bounds = await (await source.GetAsync<PdfArray>(KnownNames.Bounds).CA()).AsDoublesAsync().CA();
        var functionDecls =
            await (await source.GetAsync<PdfArray>(KnownNames.Functions).CA()).AsAsync<PdfDictionary>().CA();
        var functions = await CreateFunctionSegments(functionDecls, domain[0], bounds, encode).CA();

        var range = await source.ReadOptionalRanges(functions[0].NumberOfOutputs).CA();
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
                await functionDecls[i].CreateFunctionAsync().CA());
        }

        return functions;
    }

    private static ClosedInterval SegmentDomain(double[] bounds, int i, ClosedInterval domain) =>
        new(
            i == 0 ? domain.MinValue : bounds[i-1],
            i >= bounds.Length ? domain.MaxValue : bounds[i]);
}