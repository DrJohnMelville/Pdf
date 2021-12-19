namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class IndexedImage : DisplayImageTest
{
    public IndexedImage() : base("Nine colors in an image, based on an index")
    {
    }


    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, IndexedColorspace())
            .WithItem(KnownNames.Width, new PdfInteger(3))
            .WithItem(KnownNames.Height, new PdfInteger(3))
            .WithItem(KnownNames.BitsPerComponent, new PdfInteger(8))
            .AsStream(GenerateImage());
    }

    private static PdfArray IndexedColorspace()
    {
        return new PdfArray(
            KnownNames.Indexed, KnownNames.DeviceRGB,
            new PdfInteger(8),
            new PdfString(new byte[]
            {
                0x00, 0x00, 0x00,
                0x00, 0x00, 0xFF,
                0x00, 0xFF, 0x00,
                0x00, 0xFF, 0xFF,
                0xFF, 0x00, 0x00,
                0xFF, 0x00, 0xFF,
                0xFF, 0xFF, 0x00,
                0xFF, 0xFF, 0xFF,
                0x80, 0x80, 0xFF,
            })
        );
    }

    private byte[] GenerateImage() =>
        new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8};

}