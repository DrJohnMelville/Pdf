using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

internal interface IApplySingleFilter
{
    [Obsolete]
    ValueTask<Stream> EncodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter, object? context) =>
        throw new NotSupportedException("This method is on the way out");
    [Obsolete]
    ValueTask<Stream> DecodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter, object? context) =>
                throw new NotSupportedException("This method is on the way out");


    async ValueTask<IPdfStreamReader> EncodeAsync(IPdfStreamReader source, PdfDirectObject filter, PdfDirectObject parameter, object? context) =>
            (await this.EncodeAsync(source.AsStreamInefficient(), filter, parameter, context).CA()).AsReaderInefficient();

    async ValueTask<IPdfStreamReader> DecodeAsync(IPdfStreamReader source, PdfDirectObject filter, PdfDirectObject parameter, object? context) =>
            (await this.DecodeAsync(source.AsStreamInefficient(), filter, parameter, context).CA()).AsReaderInefficient();
}