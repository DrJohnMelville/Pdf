using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

[StaticSingleton()]
public partial class StaticSingleFilter: IApplySingleFilter
{
    public async ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter) =>
        await 
            StaticCodecFactory.CodecFor((PdfName)await filter.DirectValueAsync().CA())
                .EncodeOnReadStream(source, parameter).CA();

    public async ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter) =>
        await 
            StaticCodecFactory.CodecFor((PdfName)await filter.DirectValueAsync().CA())
                .DecodeOnReadStream(source, parameter).CA();
}