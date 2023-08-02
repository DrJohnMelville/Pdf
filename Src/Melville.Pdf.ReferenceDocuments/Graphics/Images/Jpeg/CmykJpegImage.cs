using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.Jpeg;

public class CmykJpegImage: DisplayImageTest
{
    public CmykJpegImage() : base("JPEG image encoded using CMYK")
    {
    }
    
    protected override PdfStream CreateImage()
    {
        using var img = GetType().Assembly
            .GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Graphics.Images.CMYKJpeg.jpg");
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceCMYK)
            .WithItem(KnownNames.Width, 70)
            .WithItem(KnownNames.Height,65)
            .WithItem(KnownNames.BitsPerComponent, 8)
            .WithFilter(FilterName.DCTDecode)
            .AsStream(img!, StreamFormat.DiskRepresentation);
    }
}