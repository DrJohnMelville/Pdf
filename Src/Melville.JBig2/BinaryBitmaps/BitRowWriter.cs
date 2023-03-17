using Melville.INPC;

namespace Melville.JBig2.BinaryBitmaps;

internal ref partial struct BitRowWriter
{

    [FromConstructor] private readonly byte[] array;
    [FromConstructor] private int bytePosition;
    [FromConstructor] private int bitPositionFromLSB;

    public void AssignBit(bool set)
    {
        if (set)
            SetBit();
        else
            ClearBit();
    }

    private byte SetBit() => array[bytePosition] |= (byte) (1 << bitPositionFromLSB);

    private byte ClearBit() => array[bytePosition] &= (byte) ~(1 << bitPositionFromLSB);

    public void Increment()
    {
        if (bitPositionFromLSB is 0)
        {
            bitPositionFromLSB = 7;
            bytePosition++;
        }
        else
        {
            bitPositionFromLSB--;
        }
    }
}