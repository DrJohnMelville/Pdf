using System;
using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
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
        return (PdfPage) await (await doc.PagesAsync()).GetPageAsync(0);
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
        var doc = await RoundTripPageWith(i =>
        {
            PdfObject obj = new PdfInteger(10);
            i.AddResourceObject(ResourceTypeName.XObject, name, obj);
        });
        Assert.Equal(10, ((PdfNumber?)(await doc.GetResourceAsync(ResourceTypeName.XObject, name)))?.IntValue);
    }
    [Fact]
    public async Task WithInheritedXObjectDictionary()
    {
        var name = NameDirectory.Get("N1");
        var doc = await RoundTripPageWith(j => { }, 
            i =>
            {
                PdfObject obj = new PdfInteger(10);
                i.AddResourceObject(ResourceTypeName.XObject, name, obj);
            });
        Assert.Equal(10, ((PdfNumber?)(await doc.GetResourceAsync(ResourceTypeName.XObject, name)))?.IntValue);
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
    public async Task IndirectBoxes()
    {
        var media = new PdfRect(1, 2, 3, 4);
        var crop = new PdfRect(5,6,7,8);
        var bleed = new PdfRect(9,10,11,12);
        var trim = new PdfRect(2,4,6,8);
        var art = new PdfRect(1,3,5,7);
        var doc = await RoundTripPageWith(i => { }, i =>
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
    public async Task BoxDefaults()
    {
        var media = new PdfRect(1, 2, 3, 4);
        var doc = await RoundTripPageWith(i =>
        {
            i.AddBox(BoxName.MediaBox, media);
        });
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.MediaBox));
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.CropBox));
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.BleedBox));
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.TrimBox));
        Assert.Equal(media, await doc.GetBoxAsync(BoxName.ArtBox));
        Assert.False(doc.LowLevel.ContainsKey(KnownNames.Contents));
    }

    [Fact]
    public async Task AddBuiltinFont()
    {
        PdfName fontName = KnownNames.Type;
        var page = await RoundTripPageWith(i =>
            fontName = i.AddStandardFont("F1", BuiltInFontName.CourierBoldOblique, FontEncodingName.WinAnsiEncoding));
        var res = await page.LowLevel.GetAsync<PdfDictionary>(KnownNames.Resources);
        var fonts = await res.GetAsync<PdfDictionary>(KnownNames.Font);
        var font = await fonts.GetAsync<PdfDictionary>(fontName);
        Assert.Equal(await font.GetAsync<PdfName>(KnownNames.Type), KnownNames.Font);
        Assert.Equal(await font.GetAsync<PdfName>(KnownNames.Subtype), KnownNames.Type1);
        Assert.Equal(await font.GetAsync<PdfName>(KnownNames.Name), fontName);
        Assert.Equal(await font.GetAsync<PdfName>(KnownNames.Encoding), FontEncodingName.WinAnsiEncoding);  
    }

    [Fact] 
    public async Task LiteratContentStream()
    {
        var doc = await RoundTripPageWith(i => i.AddToContentStream(new DictionaryBuilder(), "xxyyy"));
        var stream = await doc.GetContentBytes();
        var dat = await new StreamReader(stream).ReadToEndAsync();
        Assert.Equal("xxyyy", dat);
    }
    [Fact] 
    public async Task TwoContentStreams()
    {
        var doc = await RoundTripPageWith(i =>
        {
            i.AddToContentStream(new DictionaryBuilder(), "xx");
            i.AddToContentStream(new DictionaryBuilder(), "yyy");
        });
        var stream = await doc.GetContentBytes();
        var dat = await new StreamReader(stream).ReadToEndAsync();
        Assert.Equal("xxyyy", dat);
    }
}