using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.Model.Documents;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_7DocumentStructure;

public class S7_7_3_3PageAttributes
{
    private async ValueTask<PdfPage> RoundTripPageWithAsync(
        Action<PageCreator> create, Action<PageTreeNodeCreator>? parent = null)
    {
        var docCreator = new PdfDocumentCreator();
        var creator = docCreator.Pages.CreatePage();
        create(creator);
        if (parent != null) parent(docCreator.Pages);
        var doc = new PdfDocument(docCreator.CreateDocument());
        return (PdfPage) await (await doc.PagesAsync()).GetPageAsync(0);
    }
    [Fact]
    public async Task LastModifiedTimeAsync()
    {
        var dateAndTime = new DateTime(1975,07,28,1,2,3);
        var outputPage = await RoundTripPageWithAsync(i => i.AddLastModifiedTime(dateAndTime));
        Assert.Equal(dateAndTime, (await outputPage.LastModifiedAsync())!.Value.DateTime);
        
    }

    [Fact]
    public async Task LoadProcSetsAsync()
    {
        var doc = await RoundTripPageWithAsync(i => { });
        var procSets = await doc.GetProcSetsAsync();
        Assert.NotNull(procSets);
        Assert.Equal(5, procSets!.Count);
        Assert.Equal(KnownNames.PDFTName, await procSets[0]);
        Assert.Equal(KnownNames.TextTName, await procSets[1]);
        Assert.Equal(KnownNames.ImageBTName, await procSets[2]);
        Assert.Equal(KnownNames.ImageCTName, await procSets[3]);
        Assert.Equal(KnownNames.ImageITName, await procSets[4]);
    }

    [Fact]
    public async Task WithXObjectDictionaryAsync()
    {
        var name = PdfDirectObject.CreateName("N1");
        var doc = await RoundTripPageWithAsync(i =>
        {
            PdfDirectObject obj = 10;
            i.AddResourceObject(ResourceTypeName.XObject, name, obj);
        });
        Assert.Equal(10, (await doc.GetResourceAsync(ResourceTypeName.XObject, name)).Get<int>());
    }
    [Fact]
    public async Task WithInheritedXObjectDictionaryAsync()
    {
        var name = PdfDirectObject.CreateName("N1");
        var doc = await RoundTripPageWithAsync(j => { }, 
            i =>
            {
                PdfDirectObject obj = 10;
                i.AddResourceObject(ResourceTypeName.XObject, name, obj);
            });
        Assert.Equal(10, (await doc.GetResourceAsync(ResourceTypeName.XObject, name)).Get<int>());
    }

    [Fact]
    public async Task DirectBoxesAsync()
    {
        var media = new PdfRect(1, 2, 3, 4);
        var crop = new PdfRect(5,6,7,8);
        var bleed = new PdfRect(9,10,11,12);
        var trim = new PdfRect(2,4,6,8);
        var art = new PdfRect(1,3,5,7);
        var doc = await RoundTripPageWithAsync(i =>
        {
            i.AddBox(BoxName.MediaBox, media);
            i.AddBox(BoxName.CropBox, crop);
            i.AddBox(BoxName.BleedBox, bleed);
            i.AddBox(BoxName.TrimBox, trim);
            i.AddBox(BoxName.ArtBox, art);
        });
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.MediaBox));
        Assert.Equal(crop, await doc.GetBoxAsync(BoxName.CropBox));
        Assert.Equal(bleed, await doc.GetBoxAsync(BoxName.BleedBox));
        Assert.Equal(trim, await doc.GetBoxAsync(BoxName.TrimBox));
        Assert.Equal(art, await doc.GetBoxAsync(BoxName.ArtBox));
    }
    [Fact]
    public async Task IndirectBoxesAsync()
    {
        var media = new PdfRect(1, 2, 3, 4);
        var crop = new PdfRect(5,6,7,8);
        var bleed = new PdfRect(9,10,11,12);
        var trim = new PdfRect(2,4,6,8);
        var art = new PdfRect(1,3,5,7);
        var doc = await RoundTripPageWithAsync(i => { }, i =>
            {
                i.AddBox(BoxName.MediaBox, media);
                i.AddBox(BoxName.CropBox, crop);
                i.AddBox(BoxName.BleedBox, bleed);
                i.AddBox(BoxName.TrimBox, trim);
                i.AddBox(BoxName.ArtBox, art);
            });
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.MediaBox));
        Assert.Equal(crop, await doc.GetBoxAsync(BoxName.CropBox));
        Assert.Equal(bleed, await doc.GetBoxAsync(BoxName.BleedBox));
        Assert.Equal(trim, await doc.GetBoxAsync(BoxName.TrimBox));
        Assert.Equal(art, await doc.GetBoxAsync(BoxName.ArtBox));
    }
    [Fact]
    public async Task BoxDefaultsAsync()
    {
        var media = new PdfRect(1, 2, 3, 4);
        var doc = await RoundTripPageWithAsync(i =>
        {
            i.AddBox(BoxName.MediaBox, media);
        });
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.MediaBox));
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.CropBox));
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.BleedBox));
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.TrimBox));
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.ArtBox));
        Assert.False(doc.LowLevel.ContainsKey(KnownNames.ContentsTName));
    }

    [Fact]
    public async Task AddBuiltinFontAsync()
    {
        PdfDirectObject fontName = KnownNames.TypeTName;
        var page = await RoundTripPageWithAsync(i =>
            fontName = i.AddStandardFont("/F1", BuiltInFontName.CourierBoldOblique, FontEncodingName.WinAnsiEncoding));
        var res = await page.LowLevel.GetAsync<PdfDictionary>(KnownNames.ResourcesTName);
        var fonts = await res.GetAsync<PdfDictionary>(KnownNames.FontTName);
        var font = await fonts.GetAsync<PdfDictionary>(fontName);
        Assert.Equal(await font[KnownNames.TypeTName], KnownNames.FontTName);
        Assert.Equal(await font[KnownNames.SubtypeTName], KnownNames.Type1TName);
        Assert.Equal(await font[KnownNames.NameTName], fontName);
        Assert.Equal(await font[KnownNames.EncodingTName], FontEncodingName.WinAnsiEncoding);  
    }

    [Fact] 
    public async Task LiteratContentStreamAsync()
    {
        var doc = await RoundTripPageWithAsync(i => i.AddToContentStream(new DictionaryBuilder(), "xxyyy"));
        var stream = await doc.GetContentBytesAsync();
        var dat = await new StreamReader(stream).ReadToEndAsync();
        Assert.Equal("xxyyy", dat);
    }
    [Fact] 
    public async Task TwoContentStreamsAsync()
    {
        var doc = await RoundTripPageWithAsync(i =>
        {
            i.AddToContentStream(new DictionaryBuilder(), "xx");
            i.AddToContentStream(new DictionaryBuilder(), "yyy");
        });
        var stream = await doc.GetContentBytesAsync();
        var dat = await new StreamReader(stream).ReadToEndAsync();
        Assert.Equal("xxyyy", dat);
    }
}