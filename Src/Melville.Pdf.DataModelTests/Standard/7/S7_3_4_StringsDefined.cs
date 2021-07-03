using System.Diagnostics;
using System.Xml.XPath;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_3_4_StringsDefined
    {
        [Fact]
        public void StringObjectDefinitions()
        {
            var str = new PdfString("Foo Bar");
            Assert.Equal("Foo Bar", str.ToString());
        }

        [Theory]
        [InlineData("Foo", "Foo", true)]
        [InlineData("Foo", "Foos", false)]
        public void StringIsEqualMethod(string a, string b, bool areEqual)
        {
            var str = new PdfString(a);
            Assert.Equal(areEqual, b.Equals(str.ToString()));
            Assert.Equal(areEqual, str.TestEqual(b));
        }

        [Theory]
        [InlineData("<>/", "")]
        [InlineData("<20>/", " ")]
        [InlineData("<2020>/", "  ")]
        [InlineData("<202>/", "  ")]
        [InlineData("<01234567ABCDEF>/", "\x01\x23\x45\x67\xAB\xCD\xEF")]
        public void ParseHexString(string input, string output)
        {
            var reader = input.AsSequenceReader();
            var ret = HexStringParser.TryParse(ref reader, out var str);
            Assert.True(ret);
            Assert.Equal(output, str!.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData("<")]
        [InlineData("<A")]
        [InlineData("<AAA")]
        public void FailParseHexString(string data)
        {
            var seq = data.AsSequenceReader();
            Assert.False(HexStringParser.TryParse(ref seq, out _));
        }
        [Theory]
        [InlineData("")]
        [InlineData("(")]
        public void FailParseLiteralString(string data)
        {
            var seq = data.AsSequenceReader();
            Assert.False(LiteralStringParser.TryParse(ref seq, out _));
        }

        [Theory]
        [InlineData("()/", "")]
        [InlineData("(Hello)/", "Hello")]
        [InlineData("(He(l(lo)))/", "He(l(lo))")] // balanced parens are legal.
        [InlineData("(\\n)", "\n")]
        [InlineData("(\\r)", "\r")]
        [InlineData("(\\t)", "\t")]
        [InlineData("(\\b)", "\b")]
        [InlineData("(\\f)", "\f")]
        [InlineData("(\\()", "(")]
        [InlineData("(\\))", ")")]
        [InlineData("(\\\\))", "\\")]
        [InlineData("(a\r\nb))", "a\r\nb")]
        public void ParseLiteralString(string source, string result)
        {
            var seq = source.AsSequenceReader();
            var ret = LiteralStringParser.TryParse(ref seq, out var parsedString);
            Assert.True(ret);
            Assert.Equal(result, parsedString!.ToString());
        }
    }
}