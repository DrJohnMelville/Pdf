using System;
using System.IO;
using System.IO.Pipelines;
using System.Reflection;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer.Lzw
{
    public class BitWriterTest
    {
        
        [Theory]
        [InlineData("\x01", 4,"\x10")]
        [InlineData("\x07", 4,"\x70")]
        [InlineData("\x07\x07", 4,"\x77")]
        [InlineData("\x07\x07\x07", 4,"\x77\x70")]
        [InlineData("\x07\x07\x07\x07", 4,"\x77\x77")]
        [InlineData("\x07\x07\x07\x07\x07", 4,"\x77\x77\x70")]
        [InlineData("\x07", 5,"8")]
        [InlineData("\x07\x07", 5,"\x39\xC0")]
        [InlineData("\x07\x07\x07", 5,"\x39\xCE")]
        [InlineData("\x07\x07\x07\x07", 5,"\x39\xCE\x70")]
        [InlineData("\x07\x07\x07\x07\x07", 5,"\x39\xCE\x73\x80")]
        public async Task BitStringTest(string decoded, int bitsize, string encoded)
        {
            var target = new MemoryStream();
            var writer = new BitWriter(PipeWriter.Create(target));
            foreach (var character in decoded)
            {
                await writer.WriteBits((int) character, bitsize);
            }
            await writer.FinishWrite();
            Assert.Equal(encoded, target.ToArray().ExtendedAsciiString());
            
        }
    }
}