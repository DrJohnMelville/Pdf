using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Model.ShortStrings;

internal interface IPackedBytes
{
    int Length();
    bool SameAs(in ReadOnlySpan<byte> other);
    void Fill(in Span<byte> target);
    void AddToHash(ref FnvComputer hash);
}

internal readonly partial struct NinePackedBytes : IPackedBytes
{
    private readonly ulong bits;

    public NinePackedBytes(in ReadOnlySpan<byte> bits)
    {
        this.bits = Encode(bits);
    }

    private ulong Encode(in ReadOnlySpan<byte> input)
    {
        Debug.Assert(input.Length < 10);
        ulong ret = 0;
        for (int i = input.Length - 1; i >= 0; i--)
        {
            Debug.Assert(input[i] is > 0 and < 128); 
            ret <<= 7;
            ret |= input[i];
        }
        return ret;
    }

    public int Length()
    {
        int len = 0;
        for (ulong innerBits = bits; innerBits > 0; len++)
        {
            innerBits >>= 7;
        }
        return len;
    }

    public bool SameAs(in ReadOnlySpan<byte> other)
    {
        ulong localBits = bits;
        foreach (var character in other)
        {
            if (character != PopFirstByte(ref localBits)) return false;
        }
        return localBits == 0;
    }

    public void Fill(in Span<byte> target)
    {
        ulong localBits = bits;
        for (int i = 0; i < target.Length; i++)
        {
            target[i] = PopFirstByte(ref localBits);
        }
    }
    private static byte PopFirstByte(ref ulong data)
    {
        var ret = (byte) (data & 0x7F);
        data >>= 7;
        return ret;
    }

    public void AddToHash(ref FnvComputer hash)
    {
        ulong localBits = bits;
        while (localBits != 0)
        {
            hash.SingleHashStep(PopFirstByte(ref localBits));
        } 
    }
}