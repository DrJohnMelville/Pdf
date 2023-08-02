namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class HiDefImage: DisplayImageTest
{
    public HiDefImage() : base("Draw an image with 48 bit color")
    {
    }


    protected override PdfValueStream CreateImage()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName)
            .WithItem(KnownNames.WidthTName, 256)
            .WithItem(KnownNames.HeightTName, 256)
            .WithItem(KnownNames.BitsPerComponentTName, 16)
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage()
    {
        var ret = new byte[256 * 256 * 6];
        var pos = 0;
        for (int i = 0; i < 256; i++)
        for (int j = 0; j < 256; j++)
        {
            ret[pos++] = (byte)i;
            ret[pos++] = (byte)0;
            ret[pos++] = (byte)j;
            ret[pos++] = (byte)0;
            ret[pos++] = 0;
            ret[pos++] = 0;
        }

        return ret;
    }
}