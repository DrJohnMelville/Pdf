using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class HiDefImage: DisplayImageTest
{
    public HiDefImage() : base("Draw an image with 48 bit color")
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
            .WithItem(KnownNames.BitsPerComponent, 16)
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage()
    {
        var ret = new byte[256 * 256 * 6];
        var pos = 0;
        for (int i = 0; i < 256; i++)
        for (int j = 0; j < 256; j++)
        {
            ret[pos++] = (byte)i;
            ret[pos++] = (byte)0;
            ret[pos++] = (byte)j;
            ret[pos++] = (byte)0;
            ret[pos++] = 0;
            ret[pos++] = 0;
        }

        return ret;
    }
}