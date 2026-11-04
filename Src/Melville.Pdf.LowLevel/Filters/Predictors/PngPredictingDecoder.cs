using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.Predictors;

internal class PngPredictingDecoder: PngPredictingFilter
{
    private IPngPredictor predictor = PredictorFactory.Get(0);
       
    public PngPredictingDecoder(int colors, int bitsPerColor, int columns): 
        base(colors, bitsPerColor, columns)
    {
    }


    protected override bool TryGetByte(ref SpanParsingBuffer source, out byte byteToWrite)
    {
        if (Buffer.AtEndOfRow() && source.TryGetUInt8(out var predictorByte))
        {
            predictor = PredictorFactory.Get(predictorByte);
            Buffer.AdvanceToNextRow();
        }
            
        if (!source.TryGetUInt8(out byteToWrite)) return false;
        byteToWrite = predictor.Decode(Buffer.UpLeft, Buffer.Up, Buffer.Left, byteToWrite);
        Buffer.RecordColumnValue(byteToWrite);
        Buffer.AdvanceToNextColumn();
        return true;
    }
}