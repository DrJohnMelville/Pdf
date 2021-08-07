using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
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
        [InlineData("", 4,"")]
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
            await WriterTest(decoded, bitsize, encoded);
            await ReaderTest(decoded, bitsize, encoded);
        }

        private async Task ReaderTest(string decoded, int bitsize, string encoded)
        {
            var source = new MemoryStream(encoded.AsExtendedAsciiBytes());
            var sut = new BitReader(PipeReader.Create(source));
            foreach (var character in decoded)
            {
                var result = await sut.TryRead(bitsize);
                Assert.True(result.HasValue);
                Assert.Equal(character, result!.Value);
            }
            Assert.False((await sut.TryRead(9)).HasValue);
        }  
        
        private async Task WriterTest(string decoded, int bitsize, string encoded)
        {
            var target = new MemoryStream();
            var writer = new BitWriter(PipeWriter.Create(target));
            foreach (var character in decoded)
            {
                await writer.WriteBits(character, bitsize);
            }

            await writer.FinishWrite();
            Assert.Equal(encoded, target.ToArray().ExtendedAsciiString());
        }

        [Fact]
        public async Task MoreThan8Bytes()
        {
            var ms = new MemoryStream();
            var writer = new BitWriter(PipeWriter.Create(ms));
            await writer.WriteBits(256, 9);
            await writer.WriteBits(45, 9);
            await writer.WriteBits(258, 9);
            await writer.WriteBits(258, 9);
            await writer.WriteBits(65, 9);
            await writer.WriteBits(259, 9);
            await writer.WriteBits(66, 9);
            await writer.WriteBits(257, 9);
            await writer.FinishWrite();

            Assert.Equal(new byte []{0x80, 0x0B, 0x60, 0x50, 0x22, 0x0C, 0x0C, 0x85, 0x01}, 
                ms.ToArray());
            
            
        }
    }
}   