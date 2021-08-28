using System;

namespace Melville.Pdf.LowLevel.Filters.Predictors
{
    public readonly struct PredictorContext
    {
        public byte UpLeft { get; }
        public byte Left { get; }

        public PredictorContext(byte upLeft, byte left)
        {
            UpLeft = upLeft;
            Left = left;
        }

        public (PredictorContext, byte) EncodeNext(IPngPredictor predictor, byte up, byte value) =>
            (new PredictorContext(up, value), predictor.Encode(UpLeft, up, Left, value));

        public (PredictorContext context, byte byteToWrite) DecodeNext(
            IPngPredictor predictor, byte up, byte value)
        {
            var ret = predictor.Decode(UpLeft, up, Left, value);
            return (new PredictorContext(up, ret), ret);
        }
    }
}