using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.Model.Documents;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_7DocumentStructure
{
    public class S7_7_2DocumentCatalog
    {
        [Theory]
        [InlineData(1,2,"/1.2")]
        [InlineData(1,9,"/1.9")]
        [InlineData(2,0,"/2.0")]
        public async Task DefaultVersions(byte major, byte minor, string result)
        {
            var creator = new PdfDocumentCreator();
            creator.LowLevelCreator.SetVersion(major, minor);
            var doc = new PdfDocument(creator.CreateDocument());
            Assert.Equal(result, (await doc.VersionAsync()).ToString());
        }

        [Fact]
        public async Task ExplicitVersion()
        {
            var creator = new PdfDocumentCreator();
            creator.SetVersionInCatalog(3,4);
            var doc = new PdfDocument(creator.CreateDocument());
            Assert.Equal(KnownNames.Get("3.4"), await doc.VersionAsync());
        }
    }
}