using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class InvertedStencilMask : DisplayImageTest
{
    public InvertedStencilMask() : base("Draw a stencil Mask, with the inverted decodeparams")
    {
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        await csw.SetNonstrokingRgbAsync(.15, 0.73, 0.49);
        await base.DoPaintingAsync(csw);
    }

    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.Width, 3)
            .WithItem(KnownNames.Height, 3)
            .WithItem(KnownNames.ImageMask, true)
            .WithItem(KnownNames.Decode, new PdfArray(1, 0))
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage() =>
        new byte[]{
            0b01000000,
            0b10100000,
            0b01000000
        };
}