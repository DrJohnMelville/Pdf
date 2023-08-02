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

    protected override PdfValueStream CreateImage()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.WidthTName, 3)
            .WithItem(KnownNames.HeightTName, 3)
            .WithItem(KnownNames.ImageMaskTName, true)
            .WithItem(KnownNames.DecodeTName, new PdfValueArray(1, 0))
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage() =>
        new byte[]{
            0b01000000,
            0b10100000,
            0b01000000
        };
}