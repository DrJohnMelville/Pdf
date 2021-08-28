using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Melville.Pdf.LowLevel.Filters.Predictors
{
    public abstract class PngPredictingFilter: IStreamFilterDefinition
    {
        protected byte[] buffer;
        protected int column;

        protected PngPredictingFilter(int strideInBits)
        {
            column = BitsToPaddedBytes(strideInBits);
            buffer = new byte[column];
        }

        protected static int BitsToPaddedBytes(int bits) => (bits + 7)/8;

        public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
            ref SequenceReader<byte> source, ref Span<byte> destination)
        {
            int written;
            for (written = 0;
                written < destination.Length && TryGetByte(ref source, out var nextByte);
                written++)
            {
                destination[written] = nextByte;
            }

            return (source.Position, written, false);
        }

        protected abstract bool TryGetByte(ref SequenceReader<byte> source, out byte byteToWrite);

        protected bool AtEndOfRow() => column >= buffer.Length;
    }
    
    public class PngPredictingEncoder: PngPredictingFilter
    {
        private readonly byte predictorByte;
        private readonly IPngPredictor predictor;
        private PredictorContext context;

        public PngPredictingEncoder(byte predictorByte, int strideInBits): base(strideInBits)
        {
            this.predictorByte = predictorByte == 5 ? (byte)1: predictorByte;
            predictor = PredictorFactory.Get(predictorByte);
        }
        
        protected override bool TryGetByte(ref SequenceReader<byte> source, out byte byteToWrite)
        {
            if (AtEndOfRow() && source.TryPeek(out _))
            {
                byteToWrite = predictorByte;
                ResetForNewRow();
                return true;
            }


            if (!source.TryRead(out var byteRead))
            {
                byteToWrite = 0;
                return false;
            }
            (context, byteToWrite) = context.EncodeNext(predictor, buffer[column], byteRead);
            buffer[column] = byteRead;
            column++;
            return true;
        }

        private void ResetForNewRow()
        {
            context = new PredictorContext(0, 0);
            column = 0;
        }
    }
}