namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class SimpleImage: DisplayImageTest
{
    public SimpleImage() : base("Draw a simple, generated image")
    {
    }


    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
            .WithItem(KnownNames.Width, new PdfInteger(256))
            .WithItem(KnownNames.Height, new PdfInteger(256))
            .WithItem(KnownNames.BitsPerComponent, new PdfInteger(8))
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

public class SmallSimpleImage: DisplayImageTest
{
    public SmallSimpleImage() : base("Draw a small 3x3 8 bit RGB")
    {
    }


    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
            .WithItem(KnownNames.Width, new PdfInteger(3))
            .WithItem(KnownNames.Height, new PdfInteger(3))
            .WithItem(KnownNames.BitsPerComponent, new PdfInteger(8))
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage() => new byte[]
    {
        0x00, 0x00, 0x00,
        0xFF, 0x00, 0x00,
        0x00, 0xFF, 0x00,
        0xFF, 0xFF, 0x00,
        0x00, 0x00, 0xFF,
        0xFF, 0x00, 0xFF,
        0x00, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF,
        0xFF, 0x00, 0xFF,
    };
}