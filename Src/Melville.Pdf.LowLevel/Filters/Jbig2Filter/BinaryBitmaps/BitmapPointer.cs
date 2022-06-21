using System;
using Melville.INPC;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public ref partial struct BitmapPointer
{
    private static readonly byte[] zeroArray = { 0 };
    public static BitmapPointer EmptyRow => new(zeroArray.AsSpan(), ushort.MaxValue, 0);
    
    [FromConstructor]private Span<byte> bits;
    [FromConstructor]private ushort bitOffset;
    [FromConstructor] private ushort bitsLeft;
    private byte current = 0;

    partial void OnConstructed()
    {
        if (bits.Length == 0)
        {
            bitOffset = ushort.MaxValue;
        }
        else
        {
            current = bits[0];
        }

        if (bitOffset < 7)
        {
            current &= (byte)((1 << (bitOffset + 1)) - 1);
        }
    }

    public byte CurrentValue => (byte)((current >> bitOffset) & 0x01);

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
        current = bits[0];
        bitOffset = 7;
        
    }

    public BitmapPointer WithPrefixBits(int extraBits)
    {
        var ret = this with
        {
            bitOffset =(ushort) (extraBits + bitOffset),
            bitsLeft = (ushort) (extraBits + bitsLeft)
        };
        return ret;
    }

    public BitmapPointer SelectRowLength(int col, int width)
    {
        return this with { bitsLeft = (ushort)Math.Min(width - col, bitsLeft) };
    }
}