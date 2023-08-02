namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class DecodeArray: DisplayImageTest
{
    public DecodeArray() : base("Draw a generated image with a decodeArray")
    {
    }


    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName)
            .WithItem(KnownNames.WidthTName, 256)
            .WithItem(KnownNames.HeightTName, 256)
            .WithItem(KnownNames.BitsPerComponentTName, 8)
            .WithItem(KnownNames.DecodeTName, new PdfArray(
                1.0, 0.0,
                0.25, 0.75,
                0.75, 0.75
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