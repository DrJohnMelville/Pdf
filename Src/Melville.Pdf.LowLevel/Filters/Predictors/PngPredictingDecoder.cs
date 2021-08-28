using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.Predictors
{
    public class PngPredictingDecoder: PngPredictingFilter
    {
        private IPngPredictor predictor = PredictorFactory.Get(0);
        private PredictorContext context;
       
        public PngPredictingDecoder(int strideInBits): base(strideInBits)
        {
        }


        protected override bool TryGetByte(ref SequenceReader<byte> source, out byte byteToWrite)
        {
            if (AtEndOfRow() && source.TryRead(out var predictorByte))
            {
                predictor = PredictorFactory.Get(predictorByte);
                column = 0;
                context = new PredictorContext(0, 0);
            }
            
            if (!source.TryRead(out byteToWrite)) return false;
            (context, byteToWrite) = context.DecodeNext(predictor, buffer[column], byteToWrite);
            buffer[column] = byteToWrite;
            column++;
            return true;
        }
    }
}