using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.SampledFunctions;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions
{
    public readonly struct FunctionFactory
    {
        private readonly PdfDictionary source;

        public FunctionFactory(PdfDictionary source)
        {
            this.source = source;
        }

        public async ValueTask<SampledFunctionBase> CreateSampledFunc()
        {
            var domain = await ReadIntervals(KnownNames.Domain);
            var range = await ReadIntervals(KnownNames.Range);
            var size = await ReadSize();
            var encode = source.ContainsKey(KnownNames.Encode)?
                await ReadIntervals(KnownNames.Encode):
                CreateEncodeFromSize(size);
            VerifyEqualLength(domain, encode);
            var order = 
                size.All(i=>i >= 4) &&
                await source.GetOrNullAsync(KnownNames.Order) is PdfNumber num 
                ? num.IntValue : 1;

            var samples = await ReadSamples(InputPermutations(size), range);
            return order == 3 ?
                new CubicSampledFunction(domain, range, size, encode, samples):
                new LinearSampledFunction(domain, range, size, encode, samples);
        }
        
        private int InputPermutations(int[] size) => size.Aggregate(1, (a, b) => a * b);

        private void VerifyEqualLength(ClosedInterval[] arr1, ClosedInterval[] arr2)
        {
            if (arr1.Length != arr2.Length)
                throw new PdfParseException("Invalid Sampled Function Definition");
        }

        private ClosedInterval[] CreateEncodeFromSize(int[] size) => 
            size.Select(i => new ClosedInterval(00, i - 1)).ToArray();

        private async ValueTask<int[]> ReadSize()
        {
            var array = await source.GetAsync<PdfArray>(KnownNames.Size);
            var ret = new int[array.Count];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (int)(await array.GetAsync<PdfNumber>(i)).IntValue;
            }

            return ret;
        }
        
        private async ValueTask<ClosedInterval[]> ReadIntervals(PdfName name)
        {
            var array = await source.GetAsync<PdfArray>(name);
            var length = array.Count / 2;
            var ret = new ClosedInterval[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = new ClosedInterval(
                    (await array.GetAsync<PdfNumber>(2 * i)).DoubleValue,
                    (await array.GetAsync<PdfNumber>((2 * i) + 1)).DoubleValue);
            }

            return ret;
        }
        
        private async Task<double[]> ReadSamples(int inputPermutations, ClosedInterval[] range)
        {
            var decode = source.ContainsKey(KnownNames.Decode)?
                await ReadIntervals(KnownNames.Decode): range;
            var bitsPerSample = (int)(await source.GetAsync<PdfNumber>(KnownNames.BitsPerSample)).IntValue;
            var reader = await GetBitReaderForStream(bitsPerSample);
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

        private async ValueTask<BitStreamReader> GetBitReaderForStream(int bits)
        {
            if (source is not PdfStream stream)
                throw new PdfParseException("Type 0 function must be a stream");
            var reader = new BitStreamReader(await stream.StreamContentAsync(), bits);
            return reader;
        }
    }
}