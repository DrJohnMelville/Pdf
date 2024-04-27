using System.IO;
using System.Threading.Tasks;
using Melville.CSJ2K;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class JpxToPdfAdapter: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters)
    {
        throw new System.NotSupportedException();
    }

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters)
    {
        // 4/27/2024 the J2kReader has a race bug that occasionally deadlocks reads.  The
        // memory buffer does not deadlock.
        var buffer = new byte[input.Length];
        await buffer.FillBufferAsync(0, (int)input.Length, input).CA();
        return LoadImage(buffer);
    }

    private Stream LoadImage(byte[] buffer)
    {
        var independentImage = J2kReader.FromStream(new MemoryStream(buffer));
        return independentImage.NumberOfComponents == 1 ?
            new JPeg200GrayStream(independentImage) :
            new JPeg200RgbStream(independentImage);

    }
}