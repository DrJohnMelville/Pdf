namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

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
            .WithItem(KnownNames.Width, 3)
            .WithItem(KnownNames.Height, 3)
            .WithItem(KnownNames.BitsPerComponent, 8)
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