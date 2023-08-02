using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

[StaticSingleton()]
internal partial class StaticSingleFilter: IApplySingleFilter
{
    public async ValueTask<Stream> EncodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter) =>
        await 
            StaticCodecFactory.CodecFor(filter)
                .EncodeOnReadStreamAsync(source, parameter).CA();

    public async ValueTask<Stream> DecodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter) =>
        await 
            StaticCodecFactory.CodecFor(filter)
                .DecodeOnReadStreamAsync(source, parameter).CA();
}