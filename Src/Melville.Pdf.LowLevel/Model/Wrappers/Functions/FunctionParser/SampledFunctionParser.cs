using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.VariableBitEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.SampledFunctions;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

internal static class SampledFunctionParser
{
    public static async Task<SampledFunctionBase> ParseAsync( PdfValueStream source)
    {
        var domain = await source.ReadIntervalsAsync(KnownNames.DomainTName).CA();
        var range = await source.ReadIntervalsAsync(KnownNames.RangeTName).CA();
        var size = await (await source.GetAsync<PdfValueArray>(KnownNames.SizeTName).CA()).CastAsync<int>().CA();
        var encode = source.ContainsKey(KnownNames.EncodeTName)
            ? await source.ReadIntervalsAsync(KnownNames.EncodeTName).CA()
            : CreateEncodeFromSize(size);
        VerifyEqualLength(domain, encode);
        var order =
            size.All(i => i >= 4) &&
            await source.GetOrDefaultAsync(KnownNames.OrderTName,1).CA() is {} num
                ? num
                : 1;

        var samples = await ReadSamplesAsync(source, InputPermutations(size), range).CA();
        return order == 3
            ? new CubicSampledFunction(domain, range, size, encode, samples)
            : new LinearSampledFunction(domain, range, size, encode, samples);
    }

    private static int InputPermutations(int[] size) => size.Aggregate(1, (a, b) => a * b);

    private static void VerifyEqualLength(ClosedInterval[] arr1, ClosedInterval[] arr2)
    {
        if (arr1.Length != arr2.Length)
            throw new PdfParseException("Invalid Sampled Function Definition");
    }

    private static ClosedInterval[] CreateEncodeFromSize(int[] size) => 
        size.Select(i => new ClosedInterval(0, i - 1)).ToArray();

    private static async Task<double[]> ReadSamplesAsync(
        PdfValueStream source, int inputPermutations, ClosedInterval[] range)
    {
        var decode = source.ContainsKey(KnownNames.DecodeTName)?
            await source.ReadIntervalsAsync(KnownNames.DecodeTName).CA(): range;
        var bitsPerSample = 
            await source.GetAsync<int>(KnownNames.BitsPerSampleTName).CA();
        var encodedRange = new ClosedInterval(0, (1 << bitsPerSample) - 1);
        var reader = new BitStreamReader(await source.StreamContentAsync().CA(), bitsPerSample);
        var ret = new double[inputPermutations * range.Length];
        var pos = 0;
        for (int i = 0; i < inputPermutations; i++)
        {
            for (int j = 0; j < range.Length; j++)
            {
                var encoded = await reader.NextNumAsync().CA();
                var unencoded = encodedRange.MapTo(decode[j], encoded);
                ret[pos++] = unencoded;
            }
        }

        return ret;
    }
        
}