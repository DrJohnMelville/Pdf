using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

internal interface IApplySingleFilter
{
    ValueTask<Stream> EncodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter, object? context);
    ValueTask<Stream> DecodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter, object? context);

    async ValueTask<IPdfStreamReader> EncodeAsync(IPdfStreamReader source, PdfDirectObject filter, PdfDirectObject parameter, object? context) =>
            (await this.EncodeAsync(source.AsStreamInefficient(), filter, parameter, context).CA()).AsReaderInefficient();

    async ValueTask<IPdfStreamReader> DecodeAsync(IPdfStreamReader source, PdfDirectObject filter, PdfDirectObject parameter, object? context) =>
            (await this.DecodeAsync(source.AsStreamInefficient(), filter, parameter, context).CA()).AsReaderInefficient();
}