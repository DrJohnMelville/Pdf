using Melville.INPC;

namespace Melville.JBig2.BinaryBitmaps;

public partial struct BitmapPointer
{
    private static readonly byte[] zeroArray = { 0 };
    public static BitmapPointer EmptyRow => new(zeroArray.AsMemory(), int.MaxValue, 0);
    
    [FromConstructor]private Memory<byte> bits;
    [FromConstructor]private int bitOffset;
    [FromConstructor] private int bitsLeft;
    private int bitMask = 0;
    private int readPosition = 0;

    partial void OnConstructed()
    {
        if (bits.Length == 0)
        {
            bitOffset = int.MaxValue;
        }
        bitMask = (1 << (bitOffset + 1)) - 1;
    }

    public int CurrentValue => ((bits.Span[readPosition] & bitMask) >> bitOffset) & 0x01;

    public void Increment()
    {
        if (bitsLeft <= 1)
        {
            bitOffset = int.MaxValue;
            return;
        }
        bitsLeft--;
        
        if (bitOffset > 0)
        {
            bitOffset--;
            return;
        }

        readPosition++;
        bitOffset = 7;
        bitMask = 0xFF;
    }

    public BitmapPointer WithPrefixBits(int extraBits)
    {
        var ret = this with
        {
            bitOffset =(extraBits + bitOffset),
            bitsLeft = (extraBits + bitsLeft)
        };
        return ret;
    }

    public BitmapPointer SelectRowLength(int col, int width)
    {
        return this with { bitsLeft = (int)Math.Min(width - col, bitsLeft) };
    }
}