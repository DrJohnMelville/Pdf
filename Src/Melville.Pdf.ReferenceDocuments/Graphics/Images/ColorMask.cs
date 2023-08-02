
namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class ColorMask: DisplayImageTest
{
    public ColorMask() : base("Use A Mask to crop a rectangle out of an image")
    {
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
            .WithItem(KnownNames.Mask, new PdfArray(
                75, 200,
                75, 200,
                0, 255
                ))
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