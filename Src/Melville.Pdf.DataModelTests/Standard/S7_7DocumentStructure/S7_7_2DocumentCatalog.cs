using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.DataModelTests.Standard.S7_7DocumentStructure;

public class S7_7_2DocumentCatalog
{
    [Test]
    [Arguments(1,2,"1.2")]
    [Arguments(1,9,"1.9")]
    [Arguments(2,0,"2.0")]
    public async Task DefaultVersionsAsync(byte major, byte minor, string result)
    {
        var creator = new PdfDocumentCreator();
        var doc = new PdfDocument(creator.CreateDocument(major, minor));
        Assert.Equal(result, (await doc.VersionAsync()).ToString());
    }

    [Test]
    public async Task ExplicitVersionAsync()
    {
        var creator = new PdfDocumentCreator();
        creator.SetVersionInCatalog(3,4);
        var doc = new PdfDocument(creator.CreateDocument());
        Assert.Equal(PdfDirectObject.CreateName("3.4"), await doc.VersionAsync());
    }
}