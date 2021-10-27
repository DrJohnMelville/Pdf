using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions
{
    public class S7_10_2SampledFunctions
    {
        private static async Task<PdfStream> ComplexSampledFunction()
        {
            var builder = new SampledFunctionBuilder(8, SampledFunctionOrder.Cubic);
            builder.AddInput(12, (1, 10), (1, 10));
            builder.AddOutput(x => 5 * x, (5, 50), (5, 50));
            return await builder.CreateSampledFunction(new DictionaryBuilder().WithFilter(FilterName.ASCIIHexDecode));
        }

        [Fact]
        public async Task CreateFullySpecifiedFunction()
        {
            var str = await ComplexSampledFunction();
            Assert.Equal(0, (await str.GetAsync<PdfNumber>(KnownNames.FunctionType)).IntValue);
            await str.VerifyPdfDoubleArray(KnownNames.Domain, 1, 10);
            await str.VerifyPdfDoubleArray(KnownNames.Range, 5, 50);
            await str.VerifyPdfDoubleArray(KnownNames.Size, 12);
            await str.VerifyNumber(KnownNames.BitsPerSample, 8);
            await str.VerifyNumber(KnownNames.Order, 3);
            await str.VerifyPdfDoubleArray(KnownNames.Encode, 1, 10);
            await str.VerifyPdfDoubleArray(KnownNames.Decode, 5, 50);
            await StreamTest.VerifyStreamContentAsync(
                "05050A0F14191E23282D3232", await str.StreamContentAsync(StreamFormat.ImplicitEncryption));
        }

        [Theory]
        [InlineData(-200, 5)]
        [InlineData(0.9, 5)]
        [InlineData(1, 5)]
        [InlineData(3, 15)]
        [InlineData(10, 50)]
        [InlineData(2.5, 12.5)]
        [InlineData(10.1, 50)]
        [InlineData(10000, 50)]
        public async Task EvaluateFullySpecifiedFunction(double input, double output)
        {
            var str = await ComplexSampledFunction();
            var func = await str.CreateFunction();
            Assert.Equal(output, func.ComputeSingleResult(input));
            
        }

        [Theory]
        [InlineData(1,2)]
        [InlineData(1.24,3.14)]
        [InlineData(9,9)]
        public async Task TwoDimensionalTwoOutputFunction(double inputA, double inputB)
        {
            var builder = new SampledFunctionBuilder(8);
            builder.AddInput(10,(0,9));
            builder.AddInput(10,(0,9));
            builder.AddOutput((x,y)=>2*x+3*y, (0, 255));
            builder.AddOutput((x,y)=>3*x+4*y, (0, 255));
            var str = await builder.CreateSampledFunction();
            var func = await str.CreateFunction();
            var result = func.Compute(new[] { inputA, inputB });
            Assert.Equal(2*inputA + 3*inputB, result[0]);
            Assert.Equal(3*inputA + 4 * inputB, result[1]);
        }        

        [Theory]
        [InlineData(1,2)]
        [InlineData(1.24,3.14)]
        [InlineData(9,9)]
        public async Task TwoDimensionalTwoOutputFunctionWithMultiByteSamples(double inputA, double inputB)
        {
            var builder = new SampledFunctionBuilder(24);
            builder.AddInput(10,(0,9));
            builder.AddInput(10,(0,9));
            builder.AddOutput((x,y)=>2*x+3*y, (0, 255));
            builder.AddOutput((x,y)=>3*x+4*y, (0, 255));
            var str = await builder.CreateSampledFunction();
            var func = await str.CreateFunction();
            var result = func.Compute(new[] { inputA, inputB });
            Assert.Equal(2*inputA + 3*inputB, result[0]);
            Assert.Equal(3*inputA + 4 * inputB, result[1]);
        }        
        
        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 4)]
        [InlineData(5.25, 27.75)]
        [InlineData(9, 81)]
        public async Task CubicInterpolation(double inputA, double output)
        {
            var builder = new SampledFunctionBuilder(8);
            builder.AddInput(11,(0,10));
            builder.AddOutput(x=>x*x, (0, 100));
            var str = await builder.CreateSampledFunction();
            var func = await str.CreateFunction();
            Assert.Equal(output, func.ComputeSingleResult(inputA));
        }
        
        


        [Fact]
        public async Task DoNotStateUnneededOptionalArguments()
        {
            var builder = new SampledFunctionBuilder(8);
            builder.AddInput(12,(1,10));
            builder.AddOutput(x=>5*x, (0, 255));
            var str = await builder.CreateSampledFunction();
            
            Assert.False(str.ContainsKey(KnownNames.Order));
            Assert.False(str.ContainsKey(KnownNames.Encode));
            Assert.False(str.ContainsKey(KnownNames.Decode));
        }
    }
}