using System.IO;
using System.Threading.Tasks;
using Melville.CSJ2K;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class JpxToPdfAdapter: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters)
    {
        throw new System.NotSupportedException();
    }

    public ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
    {
        var independentImage = J2kReader.FromStream(input);
        return new(independentImage.NumberOfComponents == 1 ?
            new JPeg200GrayStream(independentImage):
            new JPeg200RgbStream(independentImage));
    }
}