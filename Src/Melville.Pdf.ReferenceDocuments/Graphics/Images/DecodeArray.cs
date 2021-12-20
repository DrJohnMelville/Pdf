namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class DecodeArray: DisplayImageTest
{
    public DecodeArray() : base("Draw a generated image with a decodeArray")
    {
    }


    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB)
            .WithItem(KnownNames.Width, new PdfInteger(256))
            .WithItem(KnownNames.Height, new PdfInteger(256))
            .WithItem(KnownNames.BitsPerComponent, new PdfInteger(8))
            .WithItem(KnownNames.Decode, new PdfArray(
                new PdfDouble(1.0), new PdfDouble(0.0),
                new PdfDouble(0.25), new PdfDouble(0.75),
                new PdfDouble(0.75), new PdfDouble(0.75)
            ))
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