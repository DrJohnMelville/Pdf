using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.Model.Documents;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_7DocumentStructure;

public class S7_7_3_3PageAttributes
{
    private async ValueTask<PdfPage> RoundTripPageWith(Action<PageCreator> create)
    {
        var docCreator = new PdfDocumentCreator();
        var creator = docCreator.Pages.CreatePage();
        create(creator);
        var doc = new PdfDocument(docCreator.CreateDocument());
        return await (await doc.PagesAsync()).GetPageAsync(0);
    }
    [Fact]
    public async Task LastModifiedTime()
    {
        var dateAndTime = new DateTime(1975,07,28,1,2,3);
        var outputPage = await RoundTripPageWith(i => i.AddLastModifiedTime(dateAndTime));
        Assert.Equal(dateAndTime, (await outputPage.LastModifiedAsync())!.Value.DateTime);
        
    }

    [Fact]
    public async Task LoadProcSets()
    {
        var doc = await RoundTripPageWith(i => { });
        var procSets = await doc.GetProcSetsAsync();
        Assert.Equal(5, procSets.Count);
        Assert.Equal(KnownNames.PDF, await procSets.GetAsync<PdfName>(0));
        Assert.Equal(KnownNames.Text, await procSets.GetAsync<PdfName>(1));
        Assert.Equal(KnownNames.ImageB, await procSets.GetAsync<PdfName>(2));
        Assert.Equal(KnownNames.ImageC, await procSets.GetAsync<PdfName>(3));
        Assert.Equal(KnownNames.ImageI, await procSets.GetAsync<PdfName>(4));
        
        
    }
}