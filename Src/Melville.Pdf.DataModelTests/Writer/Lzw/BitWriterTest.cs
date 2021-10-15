using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;
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
        public void BitStringTest(string decoded, int bitsize, string encoded)
        {
            WriterTest(decoded, bitsize, encoded); 
            ReaderTest(decoded, bitsize, encoded);
        }

        private void ReaderTest(string decoded, int bitsize, string encoded)
        {
            var source = new SequenceReader<byte>(new ReadOnlySequence<byte>(encoded.AsExtendedAsciiBytes()));
            var sut = new BitReader();
            foreach (var character in decoded)
            {
                var result = sut.TryRead(bitsize, ref source);
                Assert.True(result.HasValue);
                Assert.Equal(character, result!.Value);
            }
            Assert.False(sut.TryRead(9, ref source).HasValue);
        }  
        
        private void WriterTest(string decoded, int bitsize, string encoded)
        {
            var dest = new byte[50];
            int pos = 0;
            var writer = new BitWriter();
            foreach (var character in decoded)
            {
                pos += writer.WriteBits(character, bitsize, dest.AsSpan(pos));
            }

            pos += writer.FinishWrite(dest.AsSpan(pos));
            Assert.Equal(encoded, ExtendedAsciiEncoding.ExtendedAsciiString(dest.AsSpan(0, pos)));
        }

        [Fact]
        public void MoreThan8Bytes()
        {
            var ms = new byte[9];
            var pos = 0;
            var writer = new BitWriter();
            pos +=  writer.WriteBits(256, 9, ms.AsSpan(pos));
            pos +=  writer.WriteBits(45, 9, ms.AsSpan(pos));
            pos +=  writer.WriteBits(258, 9, ms.AsSpan(pos));
            pos +=  writer.WriteBits(258, 9, ms.AsSpan(pos));
            pos +=  writer.WriteBits(65, 9, ms.AsSpan(pos));
            pos +=  writer.WriteBits(259, 9, ms.AsSpan(pos));
            pos +=  writer.WriteBits(66, 9, ms.AsSpan(pos));
            pos +=  writer.WriteBits(257, 9, ms.AsSpan(pos));
            pos +=  writer.FinishWrite(ms.AsSpan(pos));

            Assert.Equal(new byte []{0x80, 0x0B, 0x60, 0x50, 0x22, 0x0C, 0x0C, 0x85, 0x01}, 
                ms);
            
            
        }
    }
}   