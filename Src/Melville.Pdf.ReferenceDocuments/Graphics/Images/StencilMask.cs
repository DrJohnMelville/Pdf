using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class StencilMask : DisplayImageTest
{
    public StencilMask() : base("Draw a stencil Mask")
    {
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        await csw.SetNonstrokingRGB(.75, 0.3, 0.19);
        await base.DoPaintingAsync(csw);
    }

    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.Width, new PdfInteger(3))
            .WithItem(KnownNames.Height, new PdfInteger(3))
            .WithItem(KnownNames.ImageMask, PdfBoolean.True)
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage() =>
        new byte[]{
            0b01000000,
            0b10100000,
            0b01000000
        };
}