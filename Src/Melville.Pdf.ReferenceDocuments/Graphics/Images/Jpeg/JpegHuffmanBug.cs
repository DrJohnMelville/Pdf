using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.Jpeg;

public class JpegHuffmanBug: DisplayImageTest
{
    public JpegHuffmanBug() : base("Draw a jpeg which exposes a bug in the jpeg decoder.")
    {
    }
    
    protected override PdfStream CreateImage()
    {
        var img = GetType().Assembly
            .GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Graphics.Images.Jpeg.JpegHuffmanBug.jpg");
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceGray)
            .WithItem(KnownNames.Width, 632)
            .WithItem(KnownNames.Height, 279)
            .WithItem(KnownNames.BitsPerComponent, 8)
            .WithFilter(FilterName.DCTDecode)
            .AsStream(img!, StreamFormat.DiskRepresentation);
    }
}