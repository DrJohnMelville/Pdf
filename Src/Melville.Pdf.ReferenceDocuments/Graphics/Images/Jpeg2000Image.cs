using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class Jpeg2000Image: DisplayImageTest
{
    public Jpeg2000Image() : base("Draw a simple, JPEG 2000 image")
    {
    }


    protected override PdfValueStream CreateImage()
    {
        using var img = GetType().Assembly
            .GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Graphics.Images.Jpeg2000.jp2");
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceGrayTName)
            .WithItem(KnownNames.WidthTName, 768)
            .WithItem(KnownNames.HeightTName, 512)
            .WithItem(KnownNames.BitsPerComponentTName, 8)
            .WithFilter(FilterName.JPXDecode)
            .AsStream(img!, StreamFormat.DiskRepresentation);
    }
}