using System;
using System.IO;
using System.Threading.Tasks;
using Melville.JpegLibrary.PipeAmdStreamAdapters;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpegFilter;

internal class DctCodec : ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters, object? context)
    {
        throw new NotSupportedException();
    }

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters, object? context) => 
        await new JpegStreamFactory( await new DctDecodeParameters(parameters).ColorTransformAsync().CA())
            .FromStreamAsync(input).CA();

}