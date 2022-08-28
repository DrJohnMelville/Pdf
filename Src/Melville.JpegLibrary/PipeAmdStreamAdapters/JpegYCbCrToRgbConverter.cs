using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

public sealed class JpegYCbCrToRgbConverter
{
    public static JpegYCbCrToRgbConverter Shared { get; } = new JpegYCbCrToRgbConverter();

    private const int ClampTableOffset = 256;
    private const int Shift = 16;
    private const int OneHalf = 1 << (Shift - 1);

    private byte[] _clampTable;
    private int[] _crRTable;
    private int[] _cbBTable;
    private int[] _crGTable;
    private int[] _cbGTable;
    private int[] _yTable;

    public JpegYCbCrToRgbConverter()
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
    private void Init(Span<float> luma, Span<float> referenceBlackWhite)
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


    private static int Fix(float x)
    {
        return (int)(x * (1L << Shift) + 0.5);
    }

    private static int Code2V(int c, float RB, float RW, float CR)
    {
        return (int)(((c - (int)RB) * CR) / ((int)(RW - RB) != 0 ? (RW - RB) : 1.0f));
    }


    public void ConvertYCbCr8ToRgba32(ReadOnlySpan<byte> ycbcr, Span<byte> rgba, int count)
    {
        if (ycbcr.Length < 3 * count)
        {
            throw new ArgumentException("YCbCr buffer is too small.", nameof(ycbcr));
        }
        if (rgba.Length < 4 * count)
        {
            throw new ArgumentException("RGBA buffer is too small.", nameof(rgba));
        }

        byte[] clampTable = _clampTable;
        int[] yTable = _yTable;

        ref byte sourceRef = ref MemoryMarshal.GetReference(ycbcr);
        ref byte destinationRef = ref MemoryMarshal.GetReference(rgba);

        byte y, cb, cr;
        int yTableValue;

        for (int i = 0; i < count; i++)
        {
            y = sourceRef;
            cb = Unsafe.Add(ref sourceRef, 1);
            cr = Unsafe.Add(ref sourceRef, 2);

            yTableValue = yTable[y];
            destinationRef = clampTable[ClampTableOffset + yTableValue + _crRTable[cr]];
            Unsafe.Add(ref destinationRef, 1) = clampTable[ClampTableOffset + yTableValue + ((_cbGTable[cb] + _crGTable[cr]) >> Shift)];
            Unsafe.Add(ref destinationRef, 2) = clampTable[ClampTableOffset + yTableValue + _cbBTable[cb]];
            Unsafe.Add(ref destinationRef, 3) = byte.MaxValue;

            sourceRef = ref Unsafe.Add(ref sourceRef, 3);
            destinationRef = ref Unsafe.Add(ref destinationRef, 4);
        }
    }

    public void ConvertYCbCr8ToRgb24(ReadOnlySpan<byte> ycbcr, Span<byte> rgb, int count)
    {
        if (ycbcr.Length < 3 * count)
        {
            throw new ArgumentException("YCbCr buffer is too small.", nameof(ycbcr));
        }
        if (rgb.Length < 3 * count)
        {
            throw new ArgumentException("RGB buffer is too small.", nameof(rgb));
        }

        byte[] clampTable = _clampTable;
        int[] yTable = _yTable;

        ref byte sourceRef = ref MemoryMarshal.GetReference(ycbcr);
        ref byte destinationRef = ref MemoryMarshal.GetReference(rgb);

        byte y, cb, cr;
        int yTableValue;

        for (int i = 0; i < count; i++)
        {
            y = sourceRef;
            cb = Unsafe.Add(ref sourceRef, 1);
            cr = Unsafe.Add(ref sourceRef, 2);

            yTableValue = yTable[y];
            destinationRef = clampTable[ClampTableOffset + yTableValue + _crRTable[cr]];
            Unsafe.Add(ref destinationRef, 1) = clampTable[ClampTableOffset + yTableValue + ((_cbGTable[cb] + _crGTable[cr]) >> Shift)];
            Unsafe.Add(ref destinationRef, 2) = clampTable[ClampTableOffset + yTableValue + _cbBTable[cb]];

            sourceRef = ref Unsafe.Add(ref sourceRef, 3);
            destinationRef = ref Unsafe.Add(ref destinationRef, 3);
        }
    }
}