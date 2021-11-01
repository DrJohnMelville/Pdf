using System;

namespace Melville.Pdf.LowLevel.Filters.Predictors;

public interface IPngPredictor
{
    int Predict(byte upperLeft, byte up, byte left);
}
    
public class NonePngPredictor:IPngPredictor
{
    public int Predict(byte upperLeft, byte up, byte left) => 0;
}
public class SubPngPredictor:IPngPredictor
{
    public int Predict(byte upperLeft, byte up, byte left) => left;
}
public class UpPngPredictor:IPngPredictor
{
    public int Predict(byte upperLeft, byte up, byte left) => up;
}
public class AveragePngPredictor:IPngPredictor
{
    public int Predict(byte upperLeft, byte up, byte left) =>
        (byte)((up+left)/2);
}
public class PaethPngPredictor:IPngPredictor
{
    public int Predict(byte upperLeft, byte up, byte left)
    {
        var composite = left + up - upperLeft;
        var leftDist = Math.Abs(composite - left);
        var topDist = Math.Abs(composite - up);
        var topLeftDist = Math.Abs(composite - upperLeft);
        //the order in which ties are broken is important to the algorithm
        if (leftDist <= topDist && leftDist <= topLeftDist) return left;
        if (topDist <= topLeftDist) return up;
        return upperLeft & 0xFF;
    }
}

public static class PredictorFactory
{
    private static readonly IPngPredictor[] decoders;
    static PredictorFactory()
    {
        var sub = new SubPngPredictor();
        decoders = new IPngPredictor[]
        {
            new NonePngPredictor(),
            sub,
            new UpPngPredictor(),
            new AveragePngPredictor(),
            new PaethPngPredictor(),
            sub // our "optimal" compression algorithm is to just use SUB every time
        };
    }

    public static IPngPredictor Get(int index) => decoders[index];
}

public static class PngPredictorOperations
{
    public static byte Encode(
        this IPngPredictor pred, byte upperLeft, byte up, byte left, byte value) =>
        (byte)(value - pred.Predict(upperLeft, up, left));
    public static byte Decode(
        this IPngPredictor pred, byte upperLeft, byte up, byte left, byte value) =>
        (byte)(value + pred.Predict(upperLeft, up, left));
}