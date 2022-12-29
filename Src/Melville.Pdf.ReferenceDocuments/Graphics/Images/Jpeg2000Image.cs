using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class Jpeg2000Image: DisplayImageTest
{
    public Jpeg2000Image() : base("Draw a simple, JPEG 2000 image")
    {
    }


    protected override PdfStream CreateImage()
    {
        using var img = GetType().Assembly
            .GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Graphics.Images.Jpeg2000.jp2");
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceGray)
            .WithItem(KnownNames.Width, 768)
            .WithItem(KnownNames.Height, 512)
            .WithItem(KnownNames.BitsPerComponent, 8)
            .WithFilter(FilterName.JPXDecode)
            .AsStream(img!, StreamFormat.DiskRepresentation);
    }
}