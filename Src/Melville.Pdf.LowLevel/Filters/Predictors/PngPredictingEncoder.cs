using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.Predictors;

public class PngPredictingEncoder: PngPredictingFilter
{
    private readonly byte predictorByte;
    private readonly IPngPredictor predictor;

    public PngPredictingEncoder(int colors, int bitsPerColor, int columns, byte predictorByte): 
        base(colors, bitsPerColor, columns)
    {
        this.predictorByte = predictorByte == 5 ? (byte)1: predictorByte;
        predictor = PredictorFactory.Get(predictorByte);
    }
        
    protected override bool TryGetByte(ref SequenceReader<byte> source, out byte byteToWrite)
    {
        if (Buffer.AtEndOfRow() && source.TryPeek(out _))
        {
            byteToWrite = predictorByte;
            Buffer.AdvanceToNextRow();
            return true;
        }


        if (!source.TryRead(out var byteRead))
        {
            byteToWrite = 0;
            return false;
        }

        Buffer.RecordColumnValue(byteRead);
        byteToWrite = predictor.Encode(Buffer.UpLeft, Buffer.Up, Buffer.Left, byteRead);
        Buffer.AdvanceToNextColumn();
        return true;
    }
}