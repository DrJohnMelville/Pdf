using System;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

/*
    just to be very optimmistic
    here is the color conversion code in python
    def Clamp(col):
    col = 255 if col>255 else col
    col = 0 if col<0 else col
    return  int(col)

def ColorConversion(Y, Cr, Cb):
    R = Cr*(2-2*.299) + Y
    B = Cb*(2-2*.114) + Y
    G = (Y - .114*B - .299*R)/.587
    return (Clamp(R+128),Clamp(G+128),Clamp(B+128) )
 */


public static class DiscreteCosineTransformation
{
    public static double GetInverseElement(int y, int x, in Matrix8x8<double> inputs)
    {
        var ret = 0.0;
        for (int u = 0; u < precision; u++)
        {
            for (int v = 0; v < precision; v++)
            {
                ret +=  inputs[v,u] * cosines[x,u] * cosines[y,v];
            }
        }

        return ReverseOffset(ret);
    }

    private static readonly Matrix8x8<double> cosines = new (CosineMatrix());

    private static double[] CosineMatrix()
    {
        var ret = new double[64];
        int pos = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                ret[pos++] = CosineFactor(i, j);
            }
        }
        return ret;
    }

    private static double CosineFactor(int x, int u) => Coefficient(u) *Math.Cos((2 * x + 1) * u * Math.PI / 16.0);
    private static double Coefficient(int u) => (u == 0 ? 1/Math.Sqrt(2): 1.0) / 2.0;
    private static double ReverseOffset(double ret) => ret + 128.0;
    private const int precision = 8;
}