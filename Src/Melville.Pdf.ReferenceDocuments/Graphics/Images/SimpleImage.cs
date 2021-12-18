using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class SimpleImage: Card3x5
{
    public SimpleImage() : base("Draw a simple, generated image image")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.XObject, NameDirectory.Get("I1"),
            new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.XObject)
                .WithItem(KnownNames.Subtype, KnownNames.Image)
                .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
                .WithItem(KnownNames.Width, new PdfInteger(256))
                .WithItem(KnownNames.Height, new PdfInteger(256))
                .WithItem(KnownNames.BitsPerComponent, new PdfInteger(8))
                .AsStream(GenerateImage())
            );
    }

    public byte[] GenerateImage()
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

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(0);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(20,40));
        csw.ModifyTransformMatrix(Matrix3x2.CreateScale(250, 150));
        csw.Rectangle(0,0,1,1);
        csw.StrokePath();
        await csw.DoAsync(NameDirectory.Get("I1"));
    }
}