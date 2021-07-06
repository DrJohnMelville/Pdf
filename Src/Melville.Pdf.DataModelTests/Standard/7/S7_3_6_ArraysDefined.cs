using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_3_6_ArraysDefined
    {
        [Theory]
        [InlineData("[] /fd", 0)]
        [InlineData("[123.5] sd", 1)]
        [InlineData("[123.5 (this is a string () inside)]/wdg", 2)]
        [InlineData("[true false null] /", 3)]
        [InlineData("[[true false] null] /", 2)]
        [InlineData("[/WIDTH /HGH /X1 /HEIGHT] /aaz", 4)]
        public void ParseArray(string src, int length)
        {
            AAssert.True(src.ParseAs(out PdfArray? obj));
            Assert.Equal(length, obj.Items.Length);
            
        }

        [Theory]
        [InlineData("[]%")]
        public void CannotFindEndOfArray(string str) =>
            Assert.False(str.ParseAs());
    }
}