using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.SampledFunctions;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser
{
    public static class SampledFunctionParser
    {
        public static async Task<SampledFunctionBase> Parse( PdfStream source)
        {
            var domain = await source.ReadIntervals(KnownNames.Domain);
            var range = await source.ReadIntervals(KnownNames.Range);
            var size = await (await source.GetAsync<PdfArray>(KnownNames.Size)).AsIntsAsync();
            var encode = source.ContainsKey(KnownNames.Encode)
                ? await source.ReadIntervals(KnownNames.Encode)
                : CreateEncodeFromSize(size);
            VerifyEqualLength(domain, encode);
            var order =
                size.All(i => i >= 4) &&
                await source.GetOrNullAsync(KnownNames.Order) is PdfNumber num
                    ? num.IntValue
                    : 1;

            var samples = await ReadSamples(source, InputPermutations(size), range);
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

        private static async Task<double[]> ReadSamples(
            PdfStream source, int inputPermutations, ClosedInterval[] range)
        {
            var decode = source.ContainsKey(KnownNames.Decode)?
                await source.ReadIntervals(KnownNames.Decode): range;
            var bitsPerSample = 
                (int)(await source.GetAsync<PdfNumber>(KnownNames.BitsPerSample)).IntValue;
            var reader = new BitStreamReader(await source.StreamContentAsync(), bitsPerSample);
            var ret = new double[inputPermutations * range.Length];
            var pos = 0;
            for (int i = 0; i < inputPermutations; i++)
            {
                for (int j = 0; j < range.Length; j++)
                {
                    ret[pos++] = decode[j].MapTo(range[j], await reader.NextNum());
                }
            }

            return ret;
        }
        
    }
}