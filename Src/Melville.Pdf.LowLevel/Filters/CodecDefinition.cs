using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters;

internal interface ICodecDefinition
{
    public ValueTask<Stream>  EncodeOnReadStreamAsync(Stream data, PdfObject? parameters);
    ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfObject parameters);
}

internal class CodecDefinition: ICodecDefinition
{
    private readonly Func<PdfObject?, ValueTask<IStreamFilterDefinition>> encoder;
    private readonly Func<PdfObject?, ValueTask<IStreamFilterDefinition>> decoder;

    public CodecDefinition(
        Func<PdfObject?, ValueTask<IStreamFilterDefinition>> encoder, 
        Func<PdfObject?, ValueTask<IStreamFilterDefinition>> decoder)
    {
        this.encoder = encoder;
        this.decoder = decoder;
    }

    public async ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfObject? parameters) =>
        ReadingFilterStream.Wrap(data, await encoder(parameters).CA());

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfObject parameters) =>
        ReadingFilterStream.Wrap(input, await decoder(parameters).CA());
}