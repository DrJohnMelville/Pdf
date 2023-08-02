using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.Jpeg;

public class JpegImage: DisplayImageTest
{
    public JpegImage() : base("Draw a simple, JPEG image")
    {
    }
    
    protected override PdfStream CreateImage()
    {
        using var img = GetType().Assembly
            .GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Graphics.Images.JPEG.jpg");
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName)
            .WithItem(KnownNames.WidthTName, 256)
            .WithItem(KnownNames.HeightTName, 256)
            .WithItem(KnownNames.BitsPerComponentTName, 8)
            .WithFilter(FilterName.DCTDecode)
            .AsStream(img!, StreamFormat.DiskRepresentation);
    }
}