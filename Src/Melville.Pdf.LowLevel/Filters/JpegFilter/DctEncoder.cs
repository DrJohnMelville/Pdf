using System;
using System.IO;
using System.Threading.Tasks;
using Melville.JpegLibrary.PipeAmdStreamAdapters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpegFilter;

public class DctDecoder : ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters)
    {
        throw new NotSupportedException();
    }

    public ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters) => 
        JpegStreamFactory.FromStream(input);

}