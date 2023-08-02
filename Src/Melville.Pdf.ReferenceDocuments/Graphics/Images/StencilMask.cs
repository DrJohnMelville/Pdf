using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class StencilMask : DisplayImageTest
{
    public StencilMask() : base("Draw a stencil Mask")
    {
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        await csw.SetNonstrokingRgbAsync(.75, 0.3, 0.19);
        await base.DoPaintingAsync(csw);
    }

    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.WidthTName, 3)
            .WithItem(KnownNames.HeightTName, 3)
            .WithItem(KnownNames.ImageMaskTName, true)
            .AsStream(new byte[]{
                0b01000000,
                0b10100000,
                0b01000000
            });
    }
}