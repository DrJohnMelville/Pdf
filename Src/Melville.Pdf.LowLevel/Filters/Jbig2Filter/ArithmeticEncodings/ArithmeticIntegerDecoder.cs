using System;
using System.Buffers;
using System.Diagnostics;
using System.Xml;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public class ArithmeticIntegerDecoder
{
    /*
    #warning should become an local var
    private ushort prev = 1;
 
    public int GetInteger(ref Seq source)
    {
        var sign = GetBit(ref source);
        if (LengthSelectionBitFound(ref source)) return AssembleNumber(sign, ReadInt(ref source, 2, 0));
        if (LengthSelectionBitFound(ref source)) return AssembleNumber(sign, ReadInt(ref source, 4, 4));
        if (LengthSelectionBitFound(ref source)) return AssembleNumber(sign, ReadInt(ref source, 6, 20));
        if (LengthSelectionBitFound(ref source)) return AssembleNumber(sign, ReadInt(ref source, 8, 84));
        if (LengthSelectionBitFound(ref source)) return AssembleNumber(sign, ReadInt(ref source, 12, 340));
        return AssembleNumber(sign, ReadInt(ref source, 32, 4436));
    }

    private bool LengthSelectionBitFound(ref BitSource source) => GetBit(ref source) == 0;

    private int AssembleNumber(int sign, int magnitude) =>
        (sign, magnitude) switch
        {
            (1, 0) => int.MaxValue,
            (1, _) => -magnitude,
            _ => magnitude
        };

    private int ReadInt(ref BitSource source, int bits, int offset)
    {
        int value = 0;
        for (int i = 0; i < bits; i++)
        {
            value <<= 1;
            value |= GetBit(ref source);
        }
        return value + offset;
    }

    private int GetBit(ref BitSource source)
    {
 //       GetDecoder(ref source).GetBit(ref source, prev);
 throw new NotImplementedException("working on this");
    }

    private void UpdatePrev(int bit)
    {
        Debug.Assert(bit is 0 or 1);
        prev = (ushort)(prev < 256 ? ShiftBitIntoPrev(bit) : RemoveTopBit(ShiftBitIntoPrev(bit)));

    }
    private int RemoveTopBit(int shiftedPrev) => (shiftedPrev & 511) | 256;
    private int ShiftBitIntoPrev(int bit) => (prev << 1) | bit;

    public bool HasOutOfBandRow() => true;
    public bool IsOutOfBand(int value) => value == int.MaxValue; */
}