using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3
{
    public class S7_3_4_StringsDefined
    {
        [Fact]
        public void StringObjectDefinitions()
        {
            var str = PdfString.CreateAscii("Foo Bar");
            Assert.Equal("Foo Bar", str.ToString());
        }

        [Theory]
        [InlineData("Foo", "Foo", true)]
        [InlineData("Foo", "Foos", false)]
        public void StringIsEqualMethod(string a, string b, bool areEqual)
        {
            var str = PdfString.CreateAscii(a);
            Assert.Equal(areEqual, b.Equals(str.ToString()));
            Assert.Equal(areEqual, str.TestEqual(b));
        }

        [Theory]
        [InlineData("<>/", "")]
        [InlineData("<20>/", " ")]
        [InlineData("<2020>/", "  ")]
        [InlineData("<202>/", "  ")]
        [InlineData("<01234567ABCDEF>/", "\x01\x23\x45\x67\xAB\xCD\xEF")]
        [InlineData("<01 23  4 \r\n567   ABC \tDEF>/", "\x01\x23\x45\x67\xAB\xCD\xEF")]
        public async Task ParseHexString(string input, string output)
        {
            var str = (PdfString) await input.ParseObjectAsync();
            Assert.Equal(output, str!.ToString());
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
        [InlineData("(\r\n\\\\))", "\n\\")]
        [InlineData("(\\\\))", "\\")]
        [InlineData("(a\\\r\nb))", "ab")]
        [InlineData("(a\\\nb))", "ab")]
        [InlineData("(a\\\rb))", "ab")]
        [InlineData("(a\r\nb))", "a\nb")]
        [InlineData("(a\rb))", "a\nb")]
        [InlineData("(a\nb))", "a\nb")]
        [InlineData("(a\\1b))", "a\x0001b")]
        [InlineData("(a\\21b))", "a\x0011b")]
        [InlineData("(a\\121b))", "a\x0051b")]
        [InlineData("(a\\1212b))", "a\x00512b")]
        public async Task ParseLiteralString(string source, string result)
        {
            var parsedString = (PdfString) await source.ParseObjectAsync();
            Assert.Equal(result, parsedString.ToString());
        }
    }
}