using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
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
    }
}