
namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class ExplicitColorMask: DisplayImageTest
{
    public ExplicitColorMask() : base("Draw a Image with an explicit Mask")
    {
    }

    private PdfIndirectObject? pir = null;
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.XObject, "/Fake"u8,
            cr =>
            {
                pir = cr.Add((PdfDirectObject)new DictionaryBuilder()
                    .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
                    .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
                    .WithItem(KnownNames.WidthTName, 3)
                    .WithItem(KnownNames.HeightTName, 3)
                    .WithItem(KnownNames.ImageMaskTName, true)
                    .AsStream(new byte[]{
                        0b01000000,
                        0b10100000,
                        0b01000000
                    }));
                return PdfDirectObject.CreateNull();
            });
        base.SetPageProperties(page);
    }

    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName)
            .WithItem(KnownNames.WidthTName, 256)
            .WithItem(KnownNames.HeightTName, 256)
            .WithItem(KnownNames.BitsPerComponentTName, 8)
            .WithItem(KnownNames.MaskTName, pir??throw new InvalidOperationException())
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