using System;
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
    public struct St
    public readonly struct FunctionFactory
    {
        private readonly PdfDictionary source;

        public FunctionFactory(PdfDictionary source)
        {
            this.source = source;
        }

        public async ValueTask<PdfFunction> CreateFunction()
        {
            return ((await source.GetAsync<PdfNumber>(KnownNames.FunctionType)).IntValue) switch
            {
                0 => await ReadSampledFunction(),
                2 => await ReadExponentialFunction(),
                3 => await ReadStitchedFunction(),
                var type => throw new PdfParseException("Unknown function type: "+ type)
            };
        }

        private async ValueTask<PdfFunction> ReadStitchedFunction()
        {
            var domain = await ReadIntervals(KnownNames.Domain);
            var encode = await ReadIntervals(KnownNames.Encode);
            var bounds = await source.GetAsync<PdfArray>(KnownNames.Bounds);
            var functionDecls = await source.GetAsync<PdfArray>(KnownNames.Functions);
            var functions = await CreateFunctionSegments(functionDecls, domain, bounds, encode);

            var range = await ReadOptionalRanges(functions[0].NumberOfOutputs);
            return new StitchedFunction(domain, range, functions);
        }

        private async Task<StitchedFunctionSegment[]> CreateFunctionSegments(
            PdfArray functionDecls, ClosedInterval[] domain, PdfArray bounds, ClosedInterval[] encode)
        {
            var functions = new StitchedFunctionSegment[functionDecls.Count];
            for (int i = 0; i < functionDecls.Count; i++)
            {
                functions[i] = new StitchedFunctionSegment(
                    await SegmentDomain(domain, bounds, i),
                    encode[i],
                    await new FunctionFactory(await functionDecls.GetAsync<PdfDictionary>(i)).CreateFunction());
            }

            return functions;
        }

        private async Task<ClosedInterval> SegmentDomain(ClosedInterval[] domain, PdfArray bounds, int i)
        {
            return new ClosedInterval(i == 0 ? domain[0].MinValue : (await bounds.GetAsync<PdfNumber>(i - 1)).DoubleValue,
                i >= bounds.Count ? domain[0].MaxValue : (await bounds.GetAsync<PdfNumber>(i)).DoubleValue);
        }

        private async ValueTask<PdfFunction> ReadExponentialFunction()
        {
            var domain = await ReadIntervals(KnownNames.Domain);
            var c0 = await ReadArrayWithDefault(KnownNames.C0, 0);
            var c1 = await ReadArrayWithDefault(KnownNames.C1, 1);
            if (domain.Length != 1) throw new PdfParseException("Type 2 functions must have a single input");
            if (c0.Count != c1.Count) throw new PdfParseException("C0 and C1 must have same number of elements");
            var transforms = await CreateExponentialTransforms(c0, c1);
            var n = await source.GetAsync<PdfNumber>(KnownNames.N);
            var range = await ReadOptionalRanges(c1.Count);
            if (transforms.Length != range.Length) 
                throw new PdfParseException("Must have a range for each function");
            return new ExponentialInterpolationFunction(domain, range, transforms, n.DoubleValue);
        }

        private static async Task<ClosedInterval[]> CreateExponentialTransforms(PdfArray c0, PdfArray c1)
        {
            var transforms = new ClosedInterval[c0.Count];
            for (int i = 0; i < c0.Count; i++)
            {
                transforms[i] = new ClosedInterval(
                    (await c0.GetAsync<PdfNumber>(i)).DoubleValue, (await c1.GetAsync<PdfNumber>(i)).DoubleValue);
            }

            return transforms;
        }

        private async ValueTask<ClosedInterval[]> ReadOptionalRanges(int numberOfOutputs) =>
            source.ContainsKey(KnownNames.Range)
                ? await ReadIntervals(KnownNames.Range)
                : Enumerable.Repeat(new ClosedInterval(double.MinValue, double.MaxValue), numberOfOutputs).ToArray();

        private async Task<PdfArray> ReadArrayWithDefault(PdfName name, int defaultValue) =>
            source.ContainsKey(name)
                ? await source.GetAsync<PdfArray>(name)
                : new PdfArray(new PdfInteger(defaultValue));

        private async Task<SampledFunctionBase> ReadSampledFunction()
        {
            var domain = await ReadIntervals(KnownNames.Domain);
            var range = await ReadIntervals(KnownNames.Range);
            var size = await ReadSize();
            var encode = source.ContainsKey(KnownNames.Encode)
                ? await ReadIntervals(KnownNames.Encode)
                : CreateEncodeFromSize(size);
            VerifyEqualLength(domain, encode);
            var order =
                size.All(i => i >= 4) &&
                await source.GetOrNullAsync(KnownNames.Order) is PdfNumber num
                    ? num.IntValue
                    : 1;

            var samples = await ReadSamples(InputPermutations(size), range);
            return order == 3
                ? new CubicSampledFunction(domain, range, size, encode, samples)
                : new LinearSampledFunction(domain, range, size, encode, samples);
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