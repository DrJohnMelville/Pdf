using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing
{
#warning eventually this class ought to merge with CddeFactory.PredictionCodec
    public class StaticSingleFilter: IApplySingleFilter
    {
        public async ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter) =>
            await 
                CodecFactory.CodecFor((PdfName)await filter.DirectValue())
                    .EncodeOnReadStream(source, parameter);

        public async ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter) =>
            await 
                CodecFactory.CodecFor((PdfName)await filter.DirectValue())
                    .DecodeOnReadStream(source, parameter);
    }
}