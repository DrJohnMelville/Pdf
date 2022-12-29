using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class ExplicitColorMask: DisplayImageTest
{
    public ExplicitColorMask() : base("Draw a Image with an explicit Mask")
    {
    }

    private PdfIndirectObject? pir = null;
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.XObject, NameDirectory.Get("Fake"),
            cr =>
            {
                pir = cr.Add(new DictionaryBuilder()
                    .WithItem(KnownNames.Type, KnownNames.XObject)
                    .WithItem(KnownNames.Subtype, KnownNames.Image)
                    .WithItem(KnownNames.Width, 3)
                    .WithItem(KnownNames.Height, 3)
                    .WithItem(KnownNames.ImageMask, PdfBoolean.True)
                    .AsStream(new byte[]{
                        0b01000000,
                        0b10100000,
                        0b01000000
                    }));
                return PdfTokenValues.Null;
            });
        base.SetPageProperties(page);
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
            .WithItem(KnownNames.Mask, pir)
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