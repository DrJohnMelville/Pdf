using System;
using Melville.INPC;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public ref partial struct BitmapPointer
{
    private static readonly byte[] zeroArray = { 0 };
    public static BitmapPointer EmptyRow => new(zeroArray.AsSpan(), int.MaxValue, 0);
    
    [FromConstructor]private Span<byte> bits;
    [FromConstructor]private int bitOffset;
    [FromConstructor] private int bitsLeft;
    private byte current = 0;

    partial void OnConstructed()
    {
        if (bits.Length == 0)
        {
            bitOffset = int.MaxValue;
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
            bitOffset = int.MaxValue;
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
            bitOffset =(int) (extraBits + bitOffset),
            bitsLeft = (int) (extraBits + bitsLeft)
        };
        return ret;
    }

    public BitmapPointer SelectRowLength(int col, int width)
    {
        return this with { bitsLeft = (int)Math.Min(width - col, bitsLeft) };
    }
}