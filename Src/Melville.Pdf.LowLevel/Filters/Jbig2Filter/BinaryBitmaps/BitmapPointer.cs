using System;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public ref partial struct BitmapPointer
{
    private static readonly byte[] zeroArray = { 0 };
    public static BitmapPointer EmptyRow => new(zeroArray.AsSpan(), ushort.MaxValue, 0);
    
    [FromConstructor]private Span<byte> bits;
    [FromConstructor]private ushort bitOffset;
    [FromConstructor] private ushort bitsLeft;

    partial void OnConstructed()
    {
        if (bits.Length == 0)
        {
            bits = zeroArray.AsSpan();
            bitOffset = ushort.MaxValue;
        }
    }

    public byte CurrentValue => (byte)((bits[0] >> bitOffset) & 0x01);

    public void Increment()
    {
        if (bitsLeft <= 1)
        {
            bitOffset = ushort.MaxValue;
            return;
        }
        bitsLeft--;
        
        if (bitOffset > 0)
        {
            bitOffset--;
            return;
        }
        bits = bits[1..];
        bitOffset = 7;
        
    }
}