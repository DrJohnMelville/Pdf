using Melville.Pdf.LowLevel.Model;
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
    }
}