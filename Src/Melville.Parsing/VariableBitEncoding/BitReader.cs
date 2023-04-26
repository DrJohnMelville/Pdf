using System.Buffers;
using System.Diagnostics;

namespace Melville.Parsing.VariableBitEncoding;

/// <summary>
/// Read indicitual bits from a sequence reader.
/// </summary>
public class BitReader
{
    private uint residue;
    private int bitsRemaining;

    /// <summary>
    /// Try to read a given number of bits from a sequencereader
    /// </summary>
    /// <param name="bits">Number of bits to attempt to read.</param>
    /// <param name="input">The source data</param>
    /// <param name="value">The output value.</param>
    /// <returns>True if was able to read the given number of bits, false otherwise</returns>
    public bool TryRead(int bits, ref SequenceReader<byte> input, out int value)
    {
        var ret = TryRead(bits, ref input);
        value = ret ?? 0;
        return ret.HasValue;
    }

    /// <summary>
    /// Attempt to read a given number of bits and throw if they are not availaible
    /// </summary>
    /// <param name="bits">The number of bits to read.</param>
    /// <param name="input">The data source</param>
    /// <returns>An int comprised of the given number of bits from the input.</returns>
    /// <exception cref="InvalidDataException">There is insufficient data to get the required
    /// number of bits.</exception>
    public int ForceRead(int bits, ref SequenceReader<byte> input) =>
        TryRead(bits, ref input) ?? throw new InvalidDataException("Not enough bits");

    /// <summary>
    /// Read a given number of bits.
    /// </summary>
    /// <param name="bits">The number of bits to read.</param>
    /// <param name="input">The data source</param>
    /// <returns>An int comprised of the given number of bits from the input or null when
    /// they are available.</returns>
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

    /// <summary>
    /// Discard the partial byte data so the next read begins on the next whole byte.
    /// </summary>
    public void DiscardPartialByte()
    {
        residue = 0;
        bitsRemaining = 0;
    }
}