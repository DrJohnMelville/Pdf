using System.IO;
using Melville.Parsing.MultiplexSources;
using Xunit;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public class OffsetMultiplexedSourceTest
    {
        private readonly IMultiplexSource sut  = MultiplexSourceFactory.Create(
            [0,1,2,3,4,5,6,7,8,9,10]);

        private void VerifyRead(Stream reader, params byte[] data)
        {
            var acutal = new byte[data.Length];
            Assert.Equal(data.Length, reader.Read(acutal, 0, data.Length));
            Assert.Equal(data, acutal);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1,0)]
        [InlineData(1,1)]
        [InlineData(5, 0)]
        [InlineData(5, 2)]
        public void OffsetRead(int offset, int pos)
        {
            var sut2 = new OffsetMultiplexSource(sut, offset);
            var sum = offset + pos;
            VerifyRead(sut2.ReadFrom(pos), [(byte)sum, (byte)(sum+1), (byte)(sum+2)]);
        } 
    }
}