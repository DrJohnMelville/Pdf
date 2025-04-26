using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class Jpeg2000Image(): Jpeg2000Base(
    "Draw a simple, JPEG 2000 image", "Jpeg2000.jp2", 768, 512, KnownNames.DeviceGray)
{
}
public class Jpeg2000ColorImage(): Jpeg2000Base(
    "Draw a Color, JPEG 2000 image", "Jpeg2000Color.jp2", 128, 96, KnownNames.DeviceRGB)
{
}

public abstract class Jpeg2000Base(
    string title,
    string name,
    int width, int height,
    PdfIndirectObject colorSpace): DisplayImageTest(title)
{
    protected override PdfStream CreateImage()
    {
        var img = GetType().Assembly
            .GetManifestResourceStream($"Melville.Pdf.ReferenceDocuments.Graphics.Images.{name}");
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, colorSpace)
            .WithItem(KnownNames.Width, width)
            .WithItem(KnownNames.Height, height)
            .WithItem(KnownNames.BitsPerComponent, 8)
            .WithFilter(FilterName.JPXDecode)
            .AsStream(img!, StreamFormat.DiskRepresentation);
    }
}