
namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public abstract class IndexedImageBase : DisplayImageTest
{
    private readonly int sampleBits;
    private readonly byte[] data;
    private readonly bool? interpolate;
    protected IndexedImageBase(int sampleBits, bool? interpolate, params byte[] data) : base("Nine colors in an image, based on an index")
    {
        this.sampleBits = sampleBits;
        this.interpolate = interpolate;
        this.data = data;
    }


    protected override PdfValueStream CreateImage()
    {
        var builder = new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, IndexedColorspace())
            .WithItem(KnownNames.WidthTName, 3)
            .WithItem(KnownNames.HeightTName, 3)
            .WithItem(KnownNames.BitsPerComponentTName, sampleBits);
        if (interpolate.HasValue)
            builder.WithItem(KnownNames.InterpolateTName, interpolate.Value);
        return builder
            .AsStream(GenerateImage());
    }

    private static PdfValueArray IndexedColorspace()
    {
        return new PdfValueArray(
            KnownNames.IndexedTName, KnownNames.DeviceRGBTName,
            8,
            PdfDirectValue.CreateString(new byte[]
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