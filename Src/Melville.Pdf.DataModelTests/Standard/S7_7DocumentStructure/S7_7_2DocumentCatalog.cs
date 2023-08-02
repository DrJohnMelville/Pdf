using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.Model.Documents;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_7DocumentStructure;

public class S7_7_2DocumentCatalog
{
    [Theory]
    [InlineData(1,2,"1.2")]
    [InlineData(1,9,"1.9")]
    [InlineData(2,0,"2.0")]
    public async Task DefaultVersionsAsync(byte major, byte minor, string result)
    {
        var creator = new PdfDocumentCreator();
        var doc = new PdfDocument(creator.CreateDocument(major, minor));
        Assert.Equal(result, (await doc.VersionAsync()).ToString());
    }

    [Fact]
    public async Task ExplicitVersionAsync()
    {
        var creator = new PdfDocumentCreator();
        creator.SetVersionInCatalog(3,4);
        var doc = new PdfDocument(creator.CreateDocument());
        Assert.Equal(PdfDirectValue.CreateName("3.4"), await doc.VersionAsync());
    }
}