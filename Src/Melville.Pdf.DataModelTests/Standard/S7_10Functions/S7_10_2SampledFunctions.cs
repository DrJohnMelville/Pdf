using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions
{
    public class S7_10_2SampledFunctions
    {
        [Fact]
        public async Task CreateSimpleSampledFunction()
        {
            var builder = new SampledFunctionBuilder(8, SampledFunctionOrder.Cubic);
            builder.AddInput(12,(1,10), (1,10));
            builder.AddOutput(x=>5*x, (5,50), (0,255));
            var str = new LowLevelDocumentBuilder()
                .CreateSampledFunction(builder, (KnownNames.Filter, KnownNames.ASCIIHexDecode));
            Assert.Equal(0, (await str.GetAsync<PdfNumber>(KnownNames.FunctionType)).IntValue);
            await VerifyPdfArray(str, KnownNames.Domain, 1, 10);
            await VerifyPdfArray(str, KnownNames.Range, 5, 50);
            await VerifyPdfArray(str, KnownNames.Size, 12);
            Assert.Equal(8, (await str.GetAsync<PdfNumber>(KnownNames.BitsPerSample)).IntValue);
            Assert.Equal(3, (await str.GetAsync<PdfNumber>(KnownNames.Order)).IntValue);
            await VerifyPdfArray(str, KnownNames.Encode, 1, 10);
            await VerifyPdfArray(str, KnownNames.Decode, 0, 255);
            
        }

        [Fact]
        public void DoNotStateUnneededOptionalArguments()
        {
            var builder = new SampledFunctionBuilder(8);
            builder.AddInput(12,(1,10));
            builder.AddOutput(x=>5*x, (5,50));
            var str = new LowLevelDocumentBuilder().CreateSampledFunction(builder);
            
            Assert.False(str.ContainsKey(KnownNames.Order));
            Assert.False(str.ContainsKey(KnownNames.Encode));
            Assert.False(str.ContainsKey(KnownNames.Decode));
        }

        private static async Task VerifyPdfArray(PdfDictionary str, PdfName name, params double[] values)
        {
            var domain = await str.GetAsync<PdfArray>(name);
            Assert.Equal(domain.Count, values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                Assert.Equal(values[i], await domain.GetAsync<PdfNumber>(i));
            }
        }
    }
}