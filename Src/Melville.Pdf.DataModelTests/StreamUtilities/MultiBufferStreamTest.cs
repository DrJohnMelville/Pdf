using System;
using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;
using Xunit.Sdk;

namespace Melville.Pdf.DataModelTests.StreamUtilities
{
    public class MultiBufferStreamTest
    {
        [Fact]
        public void ZeroLengthBufferIsAnError()
        {
            Assert.Throws<ArgumentException>(()=>new MultiBufferStream(Array.Empty<byte>()));
        }

        [Fact]
        public void ReadSingleBuffer()
        {
            var sut = new MultiBufferStream("ABCDE".AsExtendedAsciiBytes());
            var ret = new byte[5];
            Assert.Equal(5, sut.Read(ret, 0, 5));
            Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
        }
        [Fact]
        public void SimpleStreamLength()
        {
            var sut = new MultiBufferStream("ABCDE".AsExtendedAsciiBytes());
            Assert.Equal(5, sut.Length);
            
        }
        [Fact]
        public void ReadInTwoParts()
        {
            var sut = new MultiBufferStream("ABCDE".AsExtendedAsciiBytes());
            Span<byte> ret = stackalloc byte[3];
            Assert.Equal(3, sut.Read(ret));
            Assert.Equal("ABC", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
            Span<byte> ret2 = stackalloc byte[2];
            Assert.Equal(2, sut.Read(ret2));
            Assert.Equal("DE", ExtendedAsciiEncoding.ExtendedAsciiString(ret2));
        }
        [Theory]
        [InlineData(3, SeekOrigin.Begin)]
        [InlineData(1, SeekOrigin.Current)]
        [InlineData(-2, SeekOrigin.End)]
        public void SeekTest(int location, SeekOrigin seekOrigin)
        {
            var sut = new MultiBufferStream("ABCDE".AsExtendedAsciiBytes());
            sut.Seek(2, SeekOrigin.Begin);
            sut.Seek(location, seekOrigin);
            Span<byte> ret2 = stackalloc byte[2];
            Assert.Equal(2, sut.Read(ret2));
            Assert.Equal("DE", ExtendedAsciiEncoding.ExtendedAsciiString(ret2));
        }

        [Theory]
        [InlineData(-5, false)]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(3, true)]
        [InlineData(5, true)]
        [InlineData(6, false)]
        [InlineData(7, false)]
        [InlineData(17, false)]
        public void ValidAndInvalidSeekTest(int location, bool valid)
        {
            var sut = new MultiBufferStream("ABCDE".AsExtendedAsciiBytes());
            if (valid)
            {
                sut.Position = location;
            }
            else
            {
                Assert.Throws<ArgumentException>(() => sut.Position = location);
            }
        }

        

        [Fact]
        public async Task ReadAsyncFromByteArray()
        {
            var sut = new MultiBufferStream("ABCDE".AsExtendedAsciiBytes());
            var ret = new byte[5];
            Assert.Equal(5, await sut.ReadAsync(ret, 0, 5));
            Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
        }
        [Fact]
        public async Task ReadAsyncFromMemory()
        {
            var sut = new MultiBufferStream("ABCDE".AsExtendedAsciiBytes());
            var ret = new byte[5];
            Assert.Equal(5, await sut.ReadAsync(ret.AsMemory()));
            Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
        }
        [Fact]
        public void ReadUpdatesPosition()
        {
            var sut = new MultiBufferStream("ABCDE".AsExtendedAsciiBytes());
            var ret = new byte[5];
            Assert.Equal(0, sut.Position);
            Assert.Equal(5, sut.Read(ret, 0, 5));
            Assert.Equal(5, sut.Position);
        }
        [Fact]
        public void WriteAndReadSingleBuffer()
        {
            var sut = new MultiBufferStream(10);
            Assert.Equal(0, sut.Length);
            sut.Write("ABCDE".AsExtendedAsciiBytes(), 0, 5);
            Assert.Equal(5, sut.Length);
            sut.Seek(0, SeekOrigin.Begin);
            var ret = new byte[10];
            Assert.Equal(5, sut.Read(ret, 0, 10));
            Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret.AsSpan(0,5)));
        }

        [Fact]
        public void WriteIntoMultipleBuffers()
        {
            var sut = new MultiBufferStream(3);
            for (int i = 0; i < 3; i++)
            {
                sut.Write("ABCDEFG".AsExtendedAsciiBytes().AsSpan());
            }
            sut.Seek(0, SeekOrigin.Begin);
            Span<byte> ret = stackalloc byte[21];
            Assert.Equal(21, sut.Read(ret));
            Assert.Equal("ABCDEFGABCDEFGABCDEFG", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
        }
        
    }
}