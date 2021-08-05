using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer.Lzw
{
    public class EncoderDictionaryTest
    {
        private EncoderDictionary dict = new EncoderDictionary();
        [Fact]
        public void GetInitialByteIsTheByte()
        {
            QueryDictionary(false, 12, 15, 259);
            QueryDictionary(true, 12, 15, 259);
        }

        [Fact]
        public void GetGetDifferentStrings()
        {
            QueryDictionary(false, 12,15,259);
            QueryDictionary(true, 12,15,259);
            QueryDictionary(false, 259,1,260);
            QueryDictionary(false, 259,2,261);

            QueryDictionary(true, 259,1,260);
            QueryDictionary(true, 259,2,261);
        }

        private void QueryDictionary(bool succeed, short rootIndex, byte nextByte, int expectedIndex)
        {
            Assert.Equal(succeed, dict.GetOrCreateNode(rootIndex, nextByte, out var newNode));
            Assert.Equal(expectedIndex, newNode);
        }
    }
}