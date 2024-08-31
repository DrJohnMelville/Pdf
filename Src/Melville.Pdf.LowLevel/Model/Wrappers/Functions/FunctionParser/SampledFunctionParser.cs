using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.VariableBitEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.SampledFunctions;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

internal static class SampledFunctionParser
{
    public static async Task<SampledFunctionBase> ParseAsync(PdfStream source)
    {
        var domain = await source.ReadIntervalsAsync(KnownNames.Domain).CA();
        var range = await source.ReadIntervalsAsync(KnownNames.Range).CA();
        var size = await (await source.GetAsync<PdfArray>(KnownNames.Size).CA()).CastAsync<int>().CA();
        var encode = source.ContainsKey(KnownNames.Encode)
            ? await source.ReadIntervalsAsync(KnownNames.Encode).CA()
            : CreateEncodeFromSize(size);
        VerifyEqualLength(domain, encode);
        var order =
            size.All(i => i >= 4) &&
            await source.GetOrDefaultAsync(KnownNames.Order,1).CA() is {} num
                ? num
                : 1;

        var samples = await ReadSamplesAsync(source, InputPermutations(size), range).CA();
        return order == 3
            ? new CubicSampledFunction(domain, range, size, encode, samples)
            : new LinearSampledFunction(domain, range, size, encode, samples);
    }

    private static int InputPermutations(IReadOnlyList<int> size) => size.Aggregate(1, (a, b) => a * b);

    private static void VerifyEqualLength(ClosedInterval[] arr1, ClosedInterval[] arr2)
    {
        if (arr1.Length != arr2.Length)
            throw new PdfParseException("Invalid Sampled Function Definition");
    }

    private static ClosedInterval[] CreateEncodeFromSize(IReadOnlyList<int> size) => 
        size.Select(i => new ClosedInterval(0, i - 1)).ToArray();

    private static async Task<double[]> ReadSamplesAsync(
        PdfStream source, int inputPermutations, ClosedInterval[] range)
    {
        var decode = source.ContainsKey(KnownNames.Decode)?
            await source.ReadIntervalsAsync(KnownNames.Decode).CA(): range;
        var bitsPerSample = 
            await source.GetAsync<int>(KnownNames.BitsPerSample).CA();
        var encodedRange = new ClosedInterval(0, (1 << bitsPerSample) - 1);
        await using var samplesStream = await source.StreamContentAsync().CA();
        var reader = new BitStreamReader(samplesStream, bitsPerSample);
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