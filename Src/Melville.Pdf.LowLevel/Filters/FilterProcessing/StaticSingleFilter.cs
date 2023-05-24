using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

[StaticSingleton()]
internal partial class StaticSingleFilter: IApplySingleFilter
{
    public async ValueTask<Stream> EncodeAsync(Stream source, PdfObject filter, PdfObject parameter) =>
        await 
            StaticCodecFactory.CodecFor((PdfName)await filter.DirectValueAsync().CA())
                .EncodeOnReadStreamAsync(source, parameter).CA();

    public async ValueTask<Stream> DecodeAsync(Stream source, PdfObject filter, PdfObject parameter) =>
        await 
            StaticCodecFactory.CodecFor((PdfName)await filter.DirectValueAsync().CA())
                .DecodeOnReadStreamAsync(source, parameter).CA();
}