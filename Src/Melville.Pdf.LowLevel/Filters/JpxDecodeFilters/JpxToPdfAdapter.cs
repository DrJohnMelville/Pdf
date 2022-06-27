using System.IO;
using System.Threading.Tasks;
using Melville.CSJ2K;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

public class JpxToPdfAdapter: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters)
    {
        throw new System.NotSupportedException();
    }

    public ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
    {
        var independentImage = J2kImage.FromStream(input);
        return new(new MemoryStream(CopyBits(independentImage.Data)));
    }

    private byte[] CopyBits(int[] independentImageData)
    {
        var data = new byte[independentImageData.Length];
        for (int i = 0; i < independentImageData.Length/3; i++)
        {
            data[3*i+0] = (byte)independentImageData[3*i+2];
            data[3*i+1] = (byte)independentImageData[3*i+1];
            data[3*i+2] = (byte)independentImageData[3*i+0];
        }

        return data;
    }
}