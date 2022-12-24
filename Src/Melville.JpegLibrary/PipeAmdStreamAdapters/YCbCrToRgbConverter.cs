using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

internal static class YCbCrToRgbConverter
{
    private const int ClampTableOffset = 256;
    private const int Shift = 16;
    private const int OneHalf = 1 << (Shift - 1);

    private static byte[] _clampTable;
    private static int[] _crRTable;
    private static int[] _cbBTable;
    private static int[] _crGTable;
    private static int[] _cbGTable;
    private static int[] _yTable;

    static YCbCrToRgbConverter()
    {
        _clampTable = new byte[4 * 256];
        _crRTable = new int[256];
        _cbBTable = new int[256];
        _crGTable = new int[256];
        _cbGTable = new int[256];
        _yTable = new int[256];

        Span<float> luma = stackalloc float[3];
        luma[0] = 299 / 1000f;
        luma[1] = 587 / 1000f;
        luma[2] = 114 / 1000f;

        Span<float> referenceBlackWhite = stackalloc float[6];
        referenceBlackWhite[0] = 0f;
        referenceBlackWhite[1] = 255f;
        referenceBlackWhite[2] = 128f;
        referenceBlackWhite[3] = 255f;
        referenceBlackWhite[4] = 128f;
        referenceBlackWhite[5] = 255f;

        Init(luma, referenceBlackWhite);
    }

    /*
        * Initialize the YCbCr->RGB conversion tables.  The conversion
        * is done according to the 6.0 spec:
        *
        *    R = Y + Cr * (2 - 2 * LumaRed)
        *    B = Y + Cb * (2 - 2 * LumaBlue)
        *    G =   Y
        *        - LumaBlue * Cb * (2 - 2 * LumaBlue) / LumaGreen
        *        - LumaRed * Cr * (2 - 2 * LumaRed) / LumaGreen
        *        * To avoid floating point arithmetic the fractional constants that
        * come out of the equations are represented as fixed point values
        * in the range 0...2^16.  We also eliminate multiplications by
        * pre-calculating possible values indexed by Cb and Cr (this code
        * assumes conversion is being done for 8-bit samples).
        */
    private static void Init(Span<float> luma, Span<float> referenceBlackWhite)
    {
        Debug.Assert(luma.Length >= 3);
        Debug.Assert(referenceBlackWhite.Length >= 6);

        byte[] clampTable = _clampTable;

        for (int i = 0; i < 256; i++)
        {
            clampTable[ClampTableOffset + i] = (byte)i;
        }

        int start = ClampTableOffset + 256;
        int stop = start + 2 * 256;

        for (int i = start; i < stop; i++)
        {
            clampTable[i] = 255;
        }

        float lumaRed = luma[0];
        float lumaGreen = luma[1];
        float lumaBlue = luma[2];

        float f1 = 2 - 2 * lumaRed;
        int d1 = Fix(f1);

        float f2 = lumaRed * f1 / lumaGreen;
        int d2 = -Fix(f2);

        float f3 = 2 - 2 * lumaBlue;
        int d3 = Fix(f3);

        float f4 = lumaBlue * f3 / lumaGreen;
        int d4 = -Fix(f4);

        /*
            * i is the actual input pixel value in the range 0..255
            * Cb and Cr values are in the range -128..127 (actually
            * they are in a range defined by the ReferenceBlackWhite
            * tag) so there is some range shifting to do here when
            * constructing tables indexed by the raw pixel data.
            */
        for (int i = 0, x = -128; i < 256; i++, x++)
        {
            int cr = Code2V(x, referenceBlackWhite[4] - 128.0f, referenceBlackWhite[5] - 128.0f, 127);
            int cb = Code2V(x, referenceBlackWhite[2] - 128.0f, referenceBlackWhite[3] - 128.0f, 127);

            _crRTable[i] = (d1 * cr + OneHalf) >> Shift;
            _cbBTable[i] = (d3 * cb + OneHalf) >> Shift;
            _crGTable[i] = d2 * cr;
            _cbGTable[i] = d4 * cb + OneHalf;
            _yTable[i] = Code2V(x + 128, referenceBlackWhite[0], referenceBlackWhite[1], 255);
        }
    }


    private static int Fix(float x) => (int)(x * (1L << Shift) + 0.5);

    private static int Code2V(int c, float RB, float RW, float CR) => 
        (int)(((c - (int)RB) * CR) / ((int)(RW - RB) != 0 ? (RW - RB) : 1.0f));

    public static (byte red, byte green, byte blue) YCbCrToRGB(byte y, byte cb, byte cr)
    {
        int yTableValue;
        yTableValue = _yTable[y];
        var red = _clampTable[ClampTableOffset + yTableValue + _crRTable[cr]];
        var green = _clampTable[ClampTableOffset + yTableValue + ((_cbGTable[cb] + _crGTable[cr]) >> Shift)];
        var blue = _clampTable[ClampTableOffset + yTableValue + _cbBTable[cb]];
        return (red, green, blue);
    }
}