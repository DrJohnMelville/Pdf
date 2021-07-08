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
            var arr = (PdfArray) (await "[ 1 0 R 1 0 obj 12 endobj ]".ParseToPdfAsync());
        }
    }
}