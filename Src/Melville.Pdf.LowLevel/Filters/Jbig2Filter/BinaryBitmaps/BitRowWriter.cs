using Melville.INPC;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public ref partial struct BitRowWriter
{
    private static readonly byte[] setMasks = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
    private static readonly byte[] clearMasks =
    {
        0b0111_1111,
        0b1011_1111,
        0b1101_1111,
        0b1110_1111,
        0b1111_0111,
        0b1111_1011,
        0b1111_1101,
        0b1111_1110,
    };
    
    
    [FromConstructor] private readonly byte[] array;
    [FromConstructor] private int bytePosition;
    [FromConstructor] private int bitPositionFromMSB;

    public void AssignBit(bool set)
    {
        if (set)
            SetBit();
        else
            ClearBit();
    }

    private byte SetBit()
    {
        return array[bytePosition] |= setMasks[bitPositionFromMSB];
    }

    private byte ClearBit()
    {
        return array[bytePosition] &= clearMasks[bitPositionFromMSB];
    }

    public void Increment()
    {
        if (bitPositionFromMSB is 7)
        {
            bitPositionFromMSB = 0;
            bytePosition++;
        }
        else
        {
            bitPositionFromMSB++;
        }
    }
}