using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions
{
    public class S7_10_5PostscriptFunctions
    {
        [Fact]
        public async Task CreatePostScriptFunction()
        {
            var builder = new PostscriptFunctionBuilder();
            builder.AddArgument((0,10));
            builder.AddOutput((0,20));
            var dict = builder.Create("2 mul", (KnownNames.Decode, KnownNames.FlateDecode));
            Assert.Equal(KnownNames.FlateDecode, await dict.GetAsync<PdfName>(KnownNames.Decode));
            await dict.VerifyNumber(KnownNames.FunctionType, 4);
            await dict.VerifyPdfDoubleArray(KnownNames.Domain, 0, 10);
            await dict.VerifyPdfDoubleArray(KnownNames.Range, 0, 20);
            Assert.Equal("2 mul", await (await dict.StreamContentAsync()).ReadAsStringAsync());
            
        }


        [Theory]
        [InlineData("1", "2", "{2 mul}")]
        [InlineData("1,3", "3", "{mul}")]
        [InlineData("10.3", "10.3", "{abs}")]
        [InlineData("-10.3", "10.3", "{abs}")]
        [InlineData("+10.3", "10.3", "{abs}")]
        [InlineData("5", "8", "{3.0 add}")]
        [InlineData("5", "2.5", "{-2.5 add}")]
        [InlineData("1,1", "2.5", "{atan}")]
        public Task PostScriptTest(string inputs, string outputs, string code) => 
            InnerPostScriptTest(GetDoubles(inputs), GetDoubles(outputs), code);

        private async Task InnerPostScriptTest(double[] inputs, double[] outputs, string code)
        {
            var func = await CreateFunction(code, inputs.Length, outputs.Length).CreateFunction();
            Assert.Equal(outputs, func.Compute(inputs));
            
        }

        private static PdfDictionary CreateFunction(string code, int inputCount, int outputCount)
        {
            var builder = new PostscriptFunctionBuilder();
            for (int i = 0; i < inputCount; i++)
            {
                builder.AddArgument((-100, 100));
            }

            for (int i = 0; i < outputCount; i++)
            {
                builder.AddOutput((-100, 100));
            }

            return builder.Create(code);
        }

        private static double[] GetDoubles(string inputs) =>
            inputs
                .Split(new[] { ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(i => double.Parse(i))
                .ToArray();
    }
}