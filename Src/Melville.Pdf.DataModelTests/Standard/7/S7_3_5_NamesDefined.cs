using System;
using System.Buffers;
using System.Text;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_3_5_NamesDefined
    {
        [Theory]
        [InlineData("Foo")]
        [InlineData("Fo\u1234o")]
        public void NameCanRenderInUtf8(string name)
        {
            Assert.Equal(name, new PdfName(name).ToString());
        }

        [Theory]
        [InlineData("Foo", "Foo", true)]
        [InlineData("Foo", "Bar", false)]
        [InlineData("Foo", "Foot", false)]
        public void EqualityTest(string a, string b, bool matches)
        {
            var nameA = new PdfName(a);
            var nameB = new PdfName(b);
            Assert.Equal(matches, nameA.Equals(nameB));
            Assert.Equal(matches, (object)nameA.Equals(nameB));
            Assert.Equal(matches, nameA.GetHashCode() == nameB.GetHashCode());
            
        }


        private static bool TryParseStringToName(string source, out PdfName? name)
        {
            var bytes = new SequenceReader<byte>(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(source)));
            var ret = NameParser.TryParse(ref bytes, out name);
            return ret;
        }

        [Theory]
        [InlineData("/ /", "")]
        [InlineData("/Foo /", "Foo")]
        public void ParseNameSucceed(string source, string result)
        {
            var ret = TryParseStringToName(source, out var name);
            Assert.True(ret);
            Assert.Equal(result, name!.ToString());
            
        }

        [Theory]
        [InlineData("")]
        [InlineData("/")]
        public void ParseNameFail( string text)
        {
            Assert.False(TryParseStringToName(text, out _));
        }
    }
}