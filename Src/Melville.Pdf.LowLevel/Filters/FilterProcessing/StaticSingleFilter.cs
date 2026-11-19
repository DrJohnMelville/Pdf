using System.IO;
using System.Threading.Tasks;
using Melville.INPC;

using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

[StaticSingleton()]
internal partial class StaticSingleFilter: IApplySingleFilter
{
    public async ValueTask<IPdfStreamReader> EncodeAsync(IPdfStreamReader source, PdfDirectObject filter, PdfDirectObject parameter,
        object? context) =>
        await 
            StaticCodecFactory.CodecFor(filter)
                .EncodeOnReadStreamAsync(source, parameter, context).CA();

    public async ValueTask<IPdfStreamReader> DecodeAsync(IPdfStreamReader source, PdfDirectObject filter, PdfDirectObject parameter,
        object? context) =>
        await 
            StaticCodecFactory.CodecFor(filter)
                .DecodeOnReadStreamAsync(source, parameter, context).CA();
}