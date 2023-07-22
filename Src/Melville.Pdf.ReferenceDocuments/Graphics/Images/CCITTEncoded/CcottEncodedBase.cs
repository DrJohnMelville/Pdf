using Melville.Parsing.VariableBitEncoding;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.CCITTEncoded;

public abstract class CcottEncodedBase: DisplayImageTest
{
    public CcottEncodedBase(string helpText) : base(helpText)
    {
    }

    protected override PdfValueStream CreateImage()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceGrayTName)
            .WithItem(KnownNames.WidthTName, 32)
            .WithItem(KnownNames.HeightTName, 16)
            .WithItem(KnownNames.BitsPerComponentTName, 1)
            .WithFilter(FilterName.CCITTFaxDecode)
            .WithFilterParam(CcittParamDictionary())
            .AsStream(GenerateImage());
    }

    protected abstract PdfValueDictionary CcittParamDictionary();

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