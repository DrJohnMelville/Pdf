using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.ReferenceDocuments.Graphics.Images;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.ReferenceDocuments.PageProperties.OptionalContent;

public class HideOptionalImageOn : HideOptionalImageOff
{
    public HideOptionalImageOn():base("Show Optional Image and content")
    {
    }

    protected override PdfDirectValue OnOrOff() => KnownNames.ONTName;
}
public class HideOptionalImageOff: DisplayImageTest
{
    public HideOptionalImageOff() : this("Hide optional image and content")
    {
    }

    public HideOptionalImageOff(string helpText) : base(helpText)
    { }

    private PdfIndirectValue ocg;

    protected override ValueTask AddContentToDocumentAsync(PdfDocumentCreator docCreator)
    {
        var usageDictionary = new ValueDictionaryBuilder()
            .WithItem(KnownNames.CreatorInfoTName, new ValueDictionaryBuilder().WithItem(KnownNames.CreatorTName,"JDM").AsDictionary())
            .AsDictionary();

        ocg = docCreator.LowLevelCreator.Add(new ValueDictionaryBuilder().WithItem(KnownNames.NameTName, "OptionalGroup")
            .WithItem(KnownNames.TypeTName, KnownNames.OCGTName)
            .WithItem(KnownNames.IntentTName, new PdfValueArray(KnownNames.ViewTName, KnownNames.DesignTName))
            .WithItem(KnownNames.UsageTName, usageDictionary)
            .AsDictionary());

        docCreator.AddToRootDictionary(KnownNames.OCPropertiesTName, new ValueDictionaryBuilder()
            .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg))
            .WithItem(KnownNames.DTName, new ValueDictionaryBuilder()
                .WithItem(OnOrOff(), new PdfValueArray(ocg))
                .AsDictionary())
            .AsDictionary()
        );
        return base.AddContentToDocumentAsync(docCreator);
    }

    protected virtual PdfDirectValue OnOrOff() => KnownNames.OFFTName;

    protected override void SetPageProperties(PageCreator page)
    {

        page.AddResourceObject(ResourceTypeName.Properties, PdfDirectValue.CreateName("OCLayer"),
            ocg!
        );
        base.SetPageProperties(page);
    }
    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        using (await csw.BeginMarkedRangeAsync(KnownNames.OCTName, PdfDirectValue.CreateName("OCLayer")))
        {
            csw.MoveTo(0,0);
            csw.LineTo(300,300);
            csw.StrokePath();
        }
        await base.DoPaintingAsync(csw);
    }

    protected override PdfValueStream CreateImage()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName)
            .WithItem(KnownNames.WidthTName, 256)
            .WithItem(KnownNames.HeightTName, 256)
            .WithItem(KnownNames.BitsPerComponentTName, 8)
            .WithItem(KnownNames.OCTName, ocg)
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