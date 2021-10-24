using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.Model.Documents;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_7DocumentStructure;

public class S7_7_3_3PageAttributes
{
    private async ValueTask<PdfPage> RoundTripPageWith(
        Action<PageCreator> create, Action<PageTreeNodeCreator>? parent = null)
    {
        var docCreator = new PdfDocumentCreator();
        var creator = docCreator.Pages.CreatePage();
        create(creator);
        if (parent != null) parent(docCreator.Pages);
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
        Assert.NotNull(procSets);
        Assert.Equal(5, procSets!.Count);
        Assert.Equal(KnownNames.PDF, await procSets.GetAsync<PdfName>(0));
        Assert.Equal(KnownNames.Text, await procSets.GetAsync<PdfName>(1));
        Assert.Equal(KnownNames.ImageB, await procSets.GetAsync<PdfName>(2));
        Assert.Equal(KnownNames.ImageC, await procSets.GetAsync<PdfName>(3));
        Assert.Equal(KnownNames.ImageI, await procSets.GetAsync<PdfName>(4));
    }

    [Fact]
    public async Task WithXObjectDictionary()
    {
        var name = NameDirectory.Get("N1");
        var doc = await RoundTripPageWith(i => i.AddXrefObjectResource(name, new PdfInteger(10)));
        Assert.Equal(10, ((PdfNumber?)(await doc.GetXrefObjectAsync(name)))?.IntValue);
    }
    [Fact]
    public async Task WithInheritedXObjectDictionary()
    {
        var name = NameDirectory.Get("N1");
        var doc = await RoundTripPageWith(j => { }, 
            i => i.AddXrefObjectResource(name, new PdfInteger(10))
        );
        Assert.Equal(10, ((PdfNumber?)(await doc.GetXrefObjectAsync(name)))?.IntValue);
    }

    [Fact]
    public async Task DirectBoxes()
    {
        var media = new PdfRect(1, 2, 3, 4);
        var crop = new PdfRect(5,6,7,8);
        var bleed = new PdfRect(9,10,11,12);
        var trim = new PdfRect(2,4,6,8);
        var art = new PdfRect(1,3,5,7);
        var doc = await RoundTripPageWith(i =>
        {
            i.AddBox(KnownNames.MediaBox, media);
            i.AddBox(KnownNames.CropBox, crop);
            i.AddBox(KnownNames.BleedBox, bleed);
            i.AddBox(KnownNames.TrimBox, trim);
            i.AddBox(KnownNames.ArtBox, art);
        });
        Assert.Equal(media, await doc.GetBoxAsync(KnownNames.MediaBox));
        Assert.Equal(crop, await doc.GetBoxAsync(KnownNames.CropBox));
        Assert.Equal(bleed, await doc.GetBoxAsync(KnownNames.BleedBox));
        Assert.Equal(trim, await doc.GetBoxAsync(KnownNames.TrimBox));
        Assert.Equal(art, await doc.GetBoxAsync(KnownNames.ArtBox));
    }
    [Fact]
    public async Task IndirectBoxes()
    {
        var media = new PdfRect(1, 2, 3, 4);
        var crop = new PdfRect(5,6,7,8);
        var bleed = new PdfRect(9,10,11,12);
        var trim = new PdfRect(2,4,6,8);
        var art = new PdfRect(1,3,5,7);
        var doc = await RoundTripPageWith(i => { }, i =>
            {
                i.AddBox(KnownNames.MediaBox, media);
                i.AddBox(KnownNames.CropBox, crop);
                i.AddBox(KnownNames.BleedBox, bleed);
                i.AddBox(KnownNames.TrimBox, trim);
                i.AddBox(KnownNames.ArtBox, art);
            });
        Assert.Equal(media, await doc.GetBoxAsync(KnownNames.MediaBox));
        Assert.Equal(crop, await doc.GetBoxAsync(KnownNames.CropBox));
        Assert.Equal(bleed, await doc.GetBoxAsync(KnownNames.BleedBox));
        Assert.Equal(trim, await doc.GetBoxAsync(KnownNames.TrimBox));
        Assert.Equal(art, await doc.GetBoxAsync(KnownNames.ArtBox));
    }
    [Fact]
    public async Task BoxDefaults()
    {
        var media = new PdfRect(1, 2, 3, 4);
        var doc = await RoundTripPageWith(i =>
        {
            i.AddBox(KnownNames.MediaBox, media);
        });
        Assert.Equal(media, await doc.GetBoxAsync(KnownNames.MediaBox));
        Assert.Equal(media, await doc.GetBoxAsync(KnownNames.CropBox));
        Assert.Equal(media, await doc.GetBoxAsync(KnownNames.BleedBox));
        Assert.Equal(media, await doc.GetBoxAsync(KnownNames.TrimBox));
        Assert.Equal(media, await doc.GetBoxAsync(KnownNames.ArtBox));
    }

    // [Fact]
    // public async Task AddBuiltinFont()
    // {
    //     var doc = RoundTripPageWith(i=>i.DeclareBuiltInFont("F1"))
    // }
    
}