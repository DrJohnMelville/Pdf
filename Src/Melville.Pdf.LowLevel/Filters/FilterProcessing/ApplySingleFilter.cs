using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing
{
    public interface IApplySingleFilter
    {
        ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter);
        ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter);
    }

    #warning eventually this class ought to merge with CddeFactory.PredictionCodec
    public class SinglePredictionFilter : IApplySingleFilter
    {
        private IApplySingleFilter innerFilter;

        public SinglePredictionFilter(IApplySingleFilter innerFilter)
        {
            this.innerFilter = innerFilter;
        }

        public async ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter)
        {
            var sourceWithPredictionApplied = 
                await CodecFactory.PredictionCodec.EncodeOnReadStream(source, parameter);
            return await innerFilter.Encode(sourceWithPredictionApplied, filter, parameter);
        }

        public async ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter)
        {
            var decodedSource = await innerFilter.Decode(source, filter, parameter);
            return await CodecFactory.PredictionCodec.DecodeOnReadStream(decodedSource, parameter);
        }
    }
    
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