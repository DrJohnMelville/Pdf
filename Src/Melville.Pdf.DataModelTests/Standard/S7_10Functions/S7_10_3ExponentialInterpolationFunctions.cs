using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions
{
    public class S7_10_3ExponentialInterpolationFunctions
    {
        [Fact]
        public async Task DeclareExponentialFunction()
        {
            var builder = new ExponentialFunctionBuilder(10);
            builder.AddFunction(5, 10);
            builder.AddFunction(15, 20);
            var dict = builder.Create();
            await dict.VerifyNumber(KnownNames.FunctionType, 2);
            await dict.VerifyPdfDoubleArray(KnownNames.Domain, 0, 1);
            Assert.False(dict.ContainsKey(KnownNames.Range));
            await dict.VerifyNumber(KnownNames.N, 10);
            await dict.VerifyPdfDoubleArray(KnownNames.C0, 5, 15);
            await dict.VerifyPdfDoubleArray(KnownNames.C1, 10, 20);
        }

        [Fact]
        public async Task DeclareWithExplicitRange()
        {
            var builder = new ExponentialFunctionBuilder(2, (2, 3));
            builder.AddFunction(0, 1);
            builder.AddFunction(0, 1, (4,9));
            var dict = builder.Create();
            await dict.VerifyPdfDoubleArray(KnownNames.Domain, 2, 3);
            await dict.VerifyPdfDoubleArray(KnownNames.Range, double.MinValue, double.MaxValue, 4, 9);
        }
        [Fact]
        public async Task DeclareWithDefaults()
        {
            var builder = new ExponentialFunctionBuilder(20);
            builder.AddFunction(0, 1);
            var dict = builder.Create();
            await dict.VerifyNumber(KnownNames.N, 20);
            await dict.VerifyPdfDoubleArray(KnownNames.Domain, 0, 1);
            Assert.False(dict.ContainsKey(KnownNames.Range));
            Assert.False(dict.ContainsKey(KnownNames.C0));
            Assert.False(dict.ContainsKey(KnownNames.C1));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(0.432)]
        public async Task DefaultFunctionIsLinearInterpolation(double value)
        {
            var builder = new ExponentialFunctionBuilder(1);
            builder.AddFunction(0,1);
            var func = await builder.Create().CreateFunction();
            Assert.Equal(value, func.ComputeSingleResult(value));
            
        }
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(0.432)]
        public async Task OffsetInterpolation(double value)
        {
            var builder = new ExponentialFunctionBuilder(1);
            builder.AddFunction(5,6);
            var func = await builder.Create().CreateFunction();
            Assert.Equal(5+value, func.ComputeSingleResult(value));
            
        }
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(0.432)]
        public async Task OffsetRangeInterpolation(double value)
        {
            var builder = new ExponentialFunctionBuilder(1);
            builder.AddFunction(5,55);
            var func = await builder.Create().CreateFunction();
            Assert.Equal(5+(50*value), func.ComputeSingleResult(value));
            
        }
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(0.432)]
        public async Task Exponentiation(double value)
        {
            var builder = new ExponentialFunctionBuilder(3);
            builder.AddFunction(0, 1);
            var func = await builder.Create().CreateFunction();
            Assert.Equal(value*value*value, func.ComputeSingleResult(value));
            
        }
    }
}