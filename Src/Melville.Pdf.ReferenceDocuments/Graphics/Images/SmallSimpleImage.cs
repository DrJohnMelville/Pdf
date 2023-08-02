namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class SmallSimpleImage: DisplayImageTest
{
    public SmallSimpleImage() : base("Draw a small 3x3 8 bit RGB")
    {
    }


    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName)
            .WithItem(KnownNames.WidthTName, 3)
            .WithItem(KnownNames.HeightTName, 3)
            .WithItem(KnownNames.BitsPerComponentTName, 8)
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