using System.Threading.Tasks;
using Melville.Hacks.Reflection;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7
{
    public class S7_3_8_StreamsDefined
    {
        private static long GetPosition(PdfStream obj) => 
            (long)(obj.GetField("source")!.GetField("sourceFilePosition")!);

        [Fact]
        public async Task ParseSimpleStream()
        {
            var obj = (PdfStream)await "<</LENGTH 6>> stream\r\n123456\r\nendstream".ParseObjectAsync();
            Assert.Equal(22, GetPosition(obj));
        }

        [Fact]
        public async Task ParseSimpleStreamAfterJump()
        {
            var parser =  "          <</LENGTH 6>> stream\r\n123456\r\nendstream".AsParsingSource();
            var obj = (PdfStream)await parser.ParseObjectAsync();
            Assert.Equal(32, GetPosition(obj));
            parser.Seek(5);
            var obj2 =  (PdfStream) await parser.ParseObjectAsync();
            Assert.Equal(32, GetPosition(obj2));
        }        
        [Fact]
        public async Task ParseStreamWithMissingData()
        {
            var obj = (PdfStream)await "<</LENGTH 6>> stream\r\n".ParseObjectAsync();
            Assert.Equal(22, GetPosition(obj));
        }        
    }
}