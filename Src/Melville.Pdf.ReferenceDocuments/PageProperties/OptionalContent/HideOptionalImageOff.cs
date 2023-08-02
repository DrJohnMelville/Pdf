using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics.Images;

namespace Melville.Pdf.ReferenceDocuments.PageProperties.OptionalContent;

public class HideOptionalImageOn : HideOptionalImageOff
{
    public HideOptionalImageOn():base("Show Optional Image and content")
    {
    }

    protected override PdfDirectObject OnOrOff() => KnownNames.ON;
}
public class HideOptionalImageOff: DisplayImageTest
{
    public HideOptionalImageOff() : this("Hide optional image and content")
    {
    }

    public HideOptionalImageOff(string helpText) : base(helpText)
    { }

    private PdfIndirectObject ocg;

    protected override ValueTask AddContentToDocumentAsync(PdfDocumentCreator docCreator)
    {
        var usageDictionary = new DictionaryBuilder()
            .WithItem(KnownNames.CreatorInfo, new DictionaryBuilder().WithItem(KnownNames.Creator,"JDM").AsDictionary())
            .AsDictionary();

        ocg = docCreator.LowLevelCreator.Add(new DictionaryBuilder().WithItem(KnownNames.Name, "OptionalGroup")
            .WithItem(KnownNames.Type, KnownNames.OCG)
            .WithItem(KnownNames.Intent, new PdfArray(KnownNames.View, KnownNames.Design))
            .WithItem(KnownNames.Usage, usageDictionary)
            .AsDictionary());

        docCreator.AddToRootDictionary(KnownNames.OCProperties, new DictionaryBuilder()
            .WithItem(KnownNames.OCGs, new PdfArray(ocg))
            .WithItem(KnownNames.D, new DictionaryBuilder()
                .WithItem(OnOrOff(), new PdfArray(ocg))
                .AsDictionary())
            .AsDictionary()
        );
        return base.AddContentToDocumentAsync(docCreator);
    }

    protected virtual PdfDirectObject OnOrOff() => KnownNames.OFF;

    protected override void SetPageProperties(PageCreator page)
    {

        page.AddResourceObject(ResourceTypeName.Properties, PdfDirectObject.CreateName("OCLayer"),
            ocg!
        );
        base.SetPageProperties(page);
    }
    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        using (await csw.BeginMarkedRangeAsync(KnownNames.OC, PdfDirectObject.CreateName("OCLayer")))
        {
            csw.MoveTo(0,0);
            csw.LineTo(300,300);
            csw.StrokePath();
        }
        await base.DoPaintingAsync(csw);
    }

    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
            .WithItem(KnownNames.Width, 256)
            .WithItem(KnownNames.Height, 256)
            .WithItem(KnownNames.BitsPerComponent, 8)
            .WithItem(KnownNames.OC, ocg)
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage()
    {
        var ret = new byte[256 * 256 * 3];
        var pos = 0;
        for (int i = 0; i < 256; i++)
        for (int j = 0; j < 256; j++)
        {
            ret[pos++] = (byte)i;
            ret[pos++] = (byte)j;
            ret[pos++] = 0;
        }
        return ret;
    }
}