using System;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.JpegLibrary.PipeAmdStreamAdapters;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpegFilter;

internal class DctCodec : ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfObject? parameters)
    {
        throw new NotSupportedException();
    }

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfObject parameters) => 
        await new JpegStreamFactory( await new DctDecodeParameters(parameters).ColorTransformAsync().CA())
            .FromStream(input).CA();

}