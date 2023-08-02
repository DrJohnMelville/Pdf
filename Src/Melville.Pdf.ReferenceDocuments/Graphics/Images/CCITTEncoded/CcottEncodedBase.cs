using Melville.Parsing.VariableBitEncoding;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.CCITTEncoded;

public abstract class CcottEncodedBase: DisplayImageTest
{
    public CcottEncodedBase(string helpText) : base(helpText)
    {
    }

    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceGray)
            .WithItem(KnownNames.Width, 32)
            .WithItem(KnownNames.Height, 16)
            .WithItem(KnownNames.BitsPerComponent, 1)
            .WithFilter(FilterName.CCITTFaxDecode)
            .WithFilterParam(CcittParamDictionary())
            .AsStream(GenerateImage());
    }

    protected abstract PdfDictionary CcittParamDictionary();

    private byte[] GenerateImage()
    {
        var ret = new byte[32*2];
        var bitWriter = new BitWriter();
        var pos = 0;

        for (int k = 0; k < 2; k++)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    var bit = j / (i+1) % 2;
                    pos += bitWriter.WriteBits((bit) & 1, 1, ret.AsSpan(pos));
                }
            }
        }
        bitWriter.FinishWrite(ret.AsSpan(pos));
        return ret;
    }
}