using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Conventions;
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


        private static async Task<PdfName> TryParseStringToName(string source)
        {
            return (PdfName) await Encoding.UTF8.GetBytes(source).ParseToPdfAsync();
        }

        [Theory]
        [InlineData("/ /", "")]
        [InlineData("/Foo /", "Foo")]
        [InlineData("/Two#20Words /", "Two Words")]
        public async Task ParseNameSucceed(string source, string result)
        {
            var name = await TryParseStringToName(source);
            Assert.Equal(result, name!.ToString());
            
        }
        
        [Fact]
        public async Task KnowNamesParseToConstants()
        {
            var n1 = await TryParseStringToName("/WIDTH ");
            var n2 = await TryParseStringToName("/WIDTH ");
            Assert.True(ReferenceEquals(KnownNames.Width, n1));
            Assert.True(ReferenceEquals(n1,n2));
        }
    }
}