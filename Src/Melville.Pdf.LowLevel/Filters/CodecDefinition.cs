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
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters, object? context);
    ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters, object? context);
}

internal partial class CodecDefinition: ICodecDefinition
{
    [FromConstructor]private readonly Func<PdfDirectObject, ValueTask<IStreamFilterDefinition>> encoder;
    [FromConstructor]private readonly Func<PdfDirectObject, ValueTask<IStreamFilterDefinition>> decoder;

    public async ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters, object? context) =>
        ReadingFilterStream.Wrap(data, await encoder(parameters).CA());

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters, object? context) =>
        ReadingFilterStream.Wrap(input, await decoder(parameters).CA());
}