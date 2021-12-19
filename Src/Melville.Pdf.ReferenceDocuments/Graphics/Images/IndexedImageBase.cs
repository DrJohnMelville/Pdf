namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public abstract class IndexedImageBase : DisplayImageTest
{
    private readonly int sampleBits;
    private readonly byte[] data;
    protected IndexedImageBase(int sampleBits, params byte[] data) : base("Nine colors in an image, based on an index")
    {
        this.sampleBits = sampleBits;
        this.data = data;
    }


    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, IndexedColorspace())
            .WithItem(KnownNames.Width, new PdfInteger(3))
            .WithItem(KnownNames.Height, new PdfInteger(3))
            .WithItem(KnownNames.BitsPerComponent, new PdfInteger(sampleBits))
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

    private byte[] GenerateImage() => data;
}