using System;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.LowLevel.Filters;

internal interface ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectValue parameters);
    ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectValue parameters);
}

internal partial class CodecDefinition: ICodecDefinition
{
    [FromConstructor]private readonly Func<PdfDirectValue, ValueTask<IStreamFilterDefinition>> encoder;
    [FromConstructor]private readonly Func<PdfDirectValue, ValueTask<IStreamFilterDefinition>> decoder;

    public async ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectValue parameters) =>
        ReadingFilterStream.Wrap(data, await encoder(parameters).CA());

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectValue parameters) =>
        ReadingFilterStream.Wrap(input, await decoder(parameters).CA());
}