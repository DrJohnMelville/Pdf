using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class SimpleImage: DisplayImageTest
{
    public SimpleImage() : base("Draw a simple, generated image")
    {
    }


    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
            .WithItem(KnownNames.Width, 256)
            .WithItem(KnownNames.Height, 256)
            .WithItem(KnownNames.BitsPerComponent, 8)
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage()
    {
        var ret = new byte[256 * 256 * 3];
        var pos = 0;
        for (int i = 0; i < 256; i++)
        for (int j = 0; j < 256; j++)
        {
            ret[pos++] = (byte)i;
            ret[pos++] = (byte)j;
            ret[pos++] = 0;
        }
        return ret;
    }
}

public class UnalignedSMask: DisplayImageTest
{
    public UnalignedSMask() : base("Draw a trasparent smasked image")
    {
    }

    private PdfIndirectObject? smask;
    protected override ValueTask AddContentToDocumentAsync(PdfDocumentCreator docCreator)
    {
        smask = docCreator.LowLevelCreator.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.Width, 2)
            .WithItem(KnownNames.Height, 2)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceGray)
            .WithItem(KnownNames.BitsPerComponent, 8)
            .AsStream(new byte[]{10,64,127,212})
        );
        return base.AddContentToDocumentAsync(docCreator);
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        await csw.SetStrokeRGB(0,0, 1);
        csw.SetLineWidth(100);
        csw.MoveTo(0,0);
        csw.LineTo(300,200);
        csw.StrokePath();
        await base.DoPaintingAsync(csw);
    }

    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
            .WithItem(KnownNames.Width, 256)
            .WithItem(KnownNames.Height, 256)
            .WithItem(KnownNames.BitsPerComponent, 8)
            .WithItem(KnownNames.SMask, smask)
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage()
    {
        var ret = new byte[256 * 256 * 3];
        var pos = 0;
        for (int i = 0; i < 256; i++)
        for (int j = 0; j < 256; j++)
        {
            ret[pos++] = (byte)i;
            ret[pos++] = (byte)j;
            ret[pos++] = 0;
        }
        return ret;
    }
}