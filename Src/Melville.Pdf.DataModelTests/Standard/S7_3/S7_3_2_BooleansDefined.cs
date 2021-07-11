using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7
{
    public sealed class S7_3_2_BooleansDefined
    {
        [Theory]
        [InlineData("true /", true)]
        [InlineData("false /", false)]
        public async Task ParseBoolSucceed(string text, bool value)
        {
            var item = (PdfBoolean) await text.ParseObjectAsync();
            Assert.Equal(value, item.Value);
            Assert.True(ReferenceEquals(value?PdfBoolean.True:PdfBoolean.False,item));
        }
    }
}