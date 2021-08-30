using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.Predictors
{
    public class PngPredictingDecoder: PngPredictingFilter
    {
        private IPngPredictor predictor = PredictorFactory.Get(0);
       
        public PngPredictingDecoder(int colors, int bitsPerColor, int columns): 
            base(colors, bitsPerColor, columns)
        {
        }


        protected override bool TryGetByte(ref SequenceReader<byte> source, out byte byteToWrite)
        {
            if (Buffer.AtEndOfRow() && source.TryRead(out var predictorByte))
            {
                predictor = PredictorFactory.Get(predictorByte);
                Buffer.AdvanceToNextRow();
            }
            
            if (!source.TryRead(out byteToWrite)) return false;
            byteToWrite = predictor.Decode(Buffer.UpLeft, Buffer.Up, Buffer.Left, byteToWrite);
            Buffer.RecordColumnValue(byteToWrite);
            Buffer.AdvanceToNextColumn();
            return true;
        }
    }
}