using System.Buffers;
using System.Diagnostics;

namespace Melville.Parsing.VariableBitEncoding;

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

    public int ForceRead(int bits, ref SequenceReader<byte> input) =>
        TryRead(bits, ref input) ?? throw new InvalidDataException("Not enough bits");
    public int? TryRead(int bits, ref SequenceReader<byte> input)
    {
        Debug.Assert(bits <= 32);
        if (!TryAccumulateEnoughBits(bits, ref input)) return null;
        bitsRemaining -= bits;
        (var ret, residue) = residue.SplitHighAndLowBits(bitsRemaining);
        return (int)ret;
    }

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
        residue = residue.AddLeastSignificantByte(newByte);
        bitsRemaining += 8;
        return true;
    }
    public void DiscardPartialByte()
    {
        residue = 0;
        bitsRemaining = 0;
    }
}