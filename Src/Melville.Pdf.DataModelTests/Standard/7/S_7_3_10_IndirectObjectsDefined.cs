using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S_7_3_10_IndirectObjectsDefined
    {
        [Fact]
        public async Task IndirectObjectInArray()
        {
            var arr = (PdfArray) (await "[ 1 0 R 1 0 obj 12 endobj ]   ".ParseToPdfAsync());
            Assert.False(true, 
                "Check if the reference referred to the concrete because right now it doesn't");
            
        }

        [Fact]
        public async Task ParseReference()
        {
            var result = (PdfIndirectReference) await "24 543 R".ParseToPdfAsync();
            Assert.Equal(24, result.Target.ObjectNumber);
            Assert.Equal(543, result.Target.GenerationNumber);
            Assert.Equal(PdfEmptyConstants.Null, result.Target.Value);
            
        }
    }
}