using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Parsing.PdfStreamHolders;
using Xunit;

namespace Melville.Pdf.DataModelTests.PdfStreamHolderTest
{
    public class ParsingSourceTest
    {
        private static MemoryStream IndexedStream()
        {
            var ret = new byte[256];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (byte)i;
            }

            return new MemoryStream(ret);
        }

        private readonly ParsingSource sut = new(IndexedStream());

        private SequencePosition ConfirmBytes(ReadOnlySequence<byte> seq, params byte[] values)
        {
            var reader = new SequenceReader<byte>(seq);
            foreach (var value in values)
            {
                AAssert.True(reader.TryRead(out var b));
                Assert.Equal(value, b);
            }

            return reader.Position;
        }
        [Fact]
        public async Task ReadFiveBytes()
        {
            var result = await sut.ReadAsync();
            var sp =ConfirmBytes(result.Buffer, 0, 1, 2, 3, 4);
            Assert.Equal(0, sut.Position);
            sut.AdvanceTo(result.Buffer, sp);
            Assert.Equal(5, sut.Position);
        }
        [Fact]
        public async Task ReadThenJump()
        {
            var result = await sut.ReadAsync();
            var sp =ConfirmBytes(result.Buffer, 0, 1, 2, 3, 4);
            Assert.Equal(0, sut.Position);
            sut.AdvanceTo(result.Buffer, sp);
            Assert.Equal(5, sut.Position);
            sut.Seek(45);
            result = await sut.ReadAsync();
            sp = ConfirmBytes(result.Buffer, 45, 46, 47, 48);
            sut.AdvanceTo(result.Buffer, sp);
            Assert.Equal(49, sut.Position);
        }
    }
}