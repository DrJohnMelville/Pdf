using System.Buffers;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

public class BitReader
{
    private uint residue;
    private int bitsRemaining;

    public bool TryRead(int bits, ref SequenceReader<byte> input, out int value)
    {
        var ret = TryRead(bits, ref input);
        value = ret ?? 0;
        return ret.HasValue;
    }
    public int? TryRead(int bits, ref SequenceReader<byte> input)
    {
        Debug.Assert(bits <= 32);
        if (!TryAccumulateEnoughBits(bits, ref input)) return null;
        return TakeHighNBits(bits);
    }

    private int TakeHighNBits(int bits)
    {
        bitsRemaining -= bits;
        var ret = AllBitsAbove();
        ClearBitsAbove();
        return ret;
    }

    private void ClearBitsAbove() => residue &= BitUtilities.Mask(bitsRemaining);

    private int AllBitsAbove() => (int)residue >> bitsRemaining;

    private bool TryAccumulateEnoughBits(int bits, ref SequenceReader<byte> input)
    {
        while (bits > bitsRemaining) 
            if (!TryReadByte(ref input)) 
                return false;
        return true;
    }

    private bool TryReadByte(ref SequenceReader<byte> input)
    {
        if (!input.TryRead(out var newByte)) return false;
        AddToBottomOfResidue(newByte);
        return true;
    }

    private void AddToBottomOfResidue(byte newByte)
    {
        residue <<= 8;
        residue |= newByte;
        bitsRemaining += 8;
    }

    public void DiscardPartialByte()
    {
        residue = 0;
        bitsRemaining = 0;
    }
}