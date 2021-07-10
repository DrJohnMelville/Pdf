using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Melville.Pdf.LowLevel.Parsing.NameParsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_3_7_DictionaryDefined
    {
        [Theory]
        [InlineData("  << /HEIGHT 213 /WIDTH 456  >>  ")]
        [InlineData("<</HEIGHT 213/WIDTH 456>>")]
        public async Task ParseSimpleDictionary(string input)
        {
            var dict = (PdfDictionary)(await input.ParseToPdfAsync());
            Assert.Equal(2, dict.RawItems.Count);
            Assert.Equal(213, ((PdfNumber)dict.RawItems[KnownNames.Height]).IntValue);
            Assert.Equal(456, ((PdfNumber)dict.RawItems[KnownNames.Width]).IntValue);
        }
        [Theory]
        [InlineData("  << >>  ", 0)]
        [InlineData("<</HEIGHT 213 /WIDTH 456 /ASPECT null >>", 2)] // nulls make the entry be ignored
        [InlineData(" << /DICT << /INNERDICT 121.22 >>>>", 1)] // dictionary can contain dictionaries
        public async Task SpecialCases(string input, int size)
        {
            var dict = (PdfDictionary)(await input.ParseToPdfAsync());
            Assert.Equal(size, dict.RawItems.Count);
        }
        [Theory]
        [InlineData("  <<  213 /HEIGHT /WIDTH 456  >>  ")]
        [InlineData("<</HEIGHT 213/WIDTH>>")]
        public Task Exceptions(string input) => 
            Assert.ThrowsAsync<PdfParseException>(input.ParseToPdfAsync);

        [Fact]
        public async Task DictionaryHandlesIndirectDereferences()
        {
            var d = (PdfDictionary) (await "<</HEIGHT true /WIDTH 1 0 R /AC 1 0 obj false endobj>>".ParseToPdfAsync());
            Assert.Equal(3, d.Count);

            Assert.True(d.ContainsKey(KnownNames.Height));
            Assert.False(d.ContainsKey(KnownNames.FormType));
            
            
        }
    }
}