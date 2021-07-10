using System;
using System.Buffers;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_2
    {
        [Theory]
        [InlineData('\x0', CharacterClass.White)]
        [InlineData('\x9', CharacterClass.White)]
        [InlineData('\xA', CharacterClass.White)]
        [InlineData('\xC', CharacterClass.White)]
        [InlineData('\xD', CharacterClass.White)]
        [InlineData('\x20', CharacterClass.White)]
        [InlineData('(', CharacterClass.Delimiter)]
        [InlineData(')', CharacterClass.Delimiter)]
        [InlineData('<', CharacterClass.Delimiter)]
        [InlineData('>', CharacterClass.Delimiter)]
        [InlineData('[', CharacterClass.Delimiter)]
        [InlineData(']', CharacterClass.Delimiter)]
        [InlineData('{', CharacterClass.Delimiter)]
        [InlineData('}', CharacterClass.Delimiter)]
        [InlineData('/', CharacterClass.Delimiter)]
        [InlineData('%', CharacterClass.Delimiter)]
        [InlineData('J', CharacterClass.Regular)]
        public void TestCharacterClass(char input, CharacterClass expected) => 
            Assert.Equal(expected, CharClassifier.Classify((byte)input));

        [Theory]
        [InlineData("[true false] 1")]
        [InlineData("[    true%this is a / % comment true\rfalse] 1")]
        [InlineData("[    true%t\nfalse ]1")]
        [InlineData("[    true%\r\nfalse   ]1")]
        [InlineData("[    true%this is a / % comment true\r\r\r\r\nfalse ]1")]
        public async Task Comment(string twoBoolString)
        {
            var arr = (PdfArray) await twoBoolString.ParseToPdfAsync();
            Assert.Equal(2, arr.RawItems.Count);
            Assert.Equal(PdfBoolean.True, arr.RawItems[0]);
            Assert.Equal(PdfBoolean.False, arr.RawItems[1]);
        }
    }
}