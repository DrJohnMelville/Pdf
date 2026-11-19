using Melville.INPC;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Melville.Pdf.LowLevel.Filters;

internal interface ICodecDefinition
{
#warning get rid of these
    ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters, object? context) =>
        throw new NotSupportedException("On the way out");
    ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters, object? context) =>
        throw new NotSupportedException("On the way out");

    async ValueTask<IPdfStreamReader> EncodeOnReadStreamAsync(IPdfStreamReader data, PdfDirectObject parameters, object? context) =>
        (await this.EncodeOnReadStreamAsync(data.AsStreamInefficient(), parameters, context).CA()).AsReaderInefficient();
    async ValueTask<IPdfStreamReader> DecodeOnReadStreamAsync(IPdfStreamReader input, PdfDirectObject parameters, object? context) =>
        (await this.DecodeOnReadStreamAsync(input.AsStreamInefficient(), parameters, context).CA()).AsReaderInefficient();
}

internal class CodecDefinition : ICodecDefinition
{
    private readonly Func<PdfDirectObject, ValueTask<IStreamFilterDefinition>> encoder;
    private readonly Func<PdfDirectObject, ValueTask<IStreamFilterDefinition>> decoder;

    public CodecDefinition(CodecDeclaration encoder, CodecDeclaration decoder)
    {
        this.encoder = encoder.Creator;
        this.decoder = decoder.Creator;
    }

    public async ValueTask<IPdfStreamReader> EncodeOnReadStreamAsync(
        IPdfStreamReader data, PdfDirectObject parameters, object? context) =>
        new PdfFilteredReader(data, await encoder(parameters).CA());

    public async ValueTask<IPdfStreamReader> DecodeOnReadStreamAsync(
        IPdfStreamReader input, PdfDirectObject parameters, object? context) =>
        new PdfFilteredReader(input, await decoder(parameters).CA());

}

internal class PdfFilteredReader(IPdfStreamReader source, IStreamFilterDefinition filter) :
    FilteredReader(source, filter), IPdfStreamReader
{
}

internal readonly struct CodecDeclaration
{
    public Func<PdfDirectObject, ValueTask<IStreamFilterDefinition>> Creator { get; }

    public CodecDeclaration(Func<PdfDirectObject, ValueTask<IStreamFilterDefinition>> creator) => 
        Creator = creator;
    public CodecDeclaration(Func<PdfDirectObject, IStreamFilterDefinition> creator) :
        this(i => ValueTask.FromResult(creator(i))){ }
    public CodecDeclaration(IStreamFilterDefinition filter): 
        this(_ => ValueTask.FromResult(filter)) {}

}