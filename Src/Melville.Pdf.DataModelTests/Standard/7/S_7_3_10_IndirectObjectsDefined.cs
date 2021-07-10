using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S_7_3_10_IndirectObjectsDefined
    {
        [Fact]
        public async Task ParseIndirectObjectInArray()
        {
            var arr = (PdfArray) (await "[ 1 0 R 1 0 obj 12 endobj ]   ".ParseToPdfAsync());
            Assert.Equal(((PdfIndirectReference)arr.RawItems[0]).Target, arr.RawItems[1]);
        }
        [Fact]
        public async Task ParseReference()
        {
            var result = (PdfIndirectReference) await "24 543 R".ParseToPdfAsync();
            Assert.Equal(24, result.Target.ObjectNumber);
            Assert.Equal(543, result.Target.GenerationNumber);
            Assert.Equal(PdfEmptyConstants.Null, result.Target.Value);
            
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("null")]
        [InlineData("1234/")]
        [InlineData("1234.5678/")]
        [InlineData("(string value)")]
        [InlineData("[1 2 3 4]  ")]
        [InlineData("<1234>  ")]
        [InlineData("<</Foo (bar)>>  ")]
        public async Task DirectObjectValueDefinition(string targetAsPdf)
        {
            var obj = await targetAsPdf.ParseToPdfAsync();
            
            Assert.True(ReferenceEquals(obj, obj.DirectValue()));

            var indirect = new PdfIndirectObject(1, 0, obj);
            Assert.True(ReferenceEquals(obj, indirect.DirectValue()));

            var reference = new PdfIndirectReference(indirect);
            Assert.True(ReferenceEquals(obj, reference.DirectValue()));
        }
    }
}