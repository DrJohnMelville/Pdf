using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;

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
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
            .WithItem(KnownNames.Width, new PdfInteger(256))
            .WithItem(KnownNames.Height, new PdfInteger(256))
            .WithItem(KnownNames.BitsPerComponent, new PdfInteger(8))
            .WithFilter(FilterName.DCTDecode)
            .AsStream(img!, StreamFormat.DiskRepresentation);
    }
}