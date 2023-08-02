using System;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters;

internal interface ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters);
    ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters);
}

internal partial class CodecDefinition: ICodecDefinition
{
    [FromConstructor]private readonly Func<PdfDirectObject, ValueTask<IStreamFilterDefinition>> encoder;
    [FromConstructor]private readonly Func<PdfDirectObject, ValueTask<IStreamFilterDefinition>> decoder;

    public async ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters) =>
        ReadingFilterStream.Wrap(data, await encoder(parameters).CA());

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters) =>
        ReadingFilterStream.Wrap(input, await decoder(parameters).CA());
}