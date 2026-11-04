using System.Diagnostics;

namespace Melville.Parsing.VariableBitEncoding;

/// <summary>
/// This class is a buffer of bits that can be written out as whole bytes.
/// </summary>
public class BitWritingBuffer
{
    private UInt128 buffer = 0;
    private int bitsUsed = 0;

    /// <summary>
    /// Add the bottom "bits" bits of "data" to the buffer to be written out.0
    /// </summary>
    /// <param name="data">Data is in the low order bits of this parameter</param>
    /// <param name="bits">Numer of bits to add</param>
    public void WriteBits(int data, int bits) => WriteBits((uint)data, bits);

    /// <summary>
    /// Add the bottom "bits" bits of "data" to the buffer to be written out.0
    /// </summary>
    /// <param name="data">Data is in the low order bits of this parameter</param>
    /// <param name="bits">Numer of bits to add</param>
    public void WriteBits(uint data, int bits)
    {
        bitsUsed += bits;
        Debug.Assert(bitsUsed <= 128);
        buffer <<= bits;
        buffer |= (UInt128)(data & (1 << bits) - 1);
    }

    /// <summary>
    /// Add enough bits so the next bit will be the MSB of a whole byte.
    /// </summary>
    public void AdvanceToWholeByte()
    {
        if (bitsUsed % 8 is > 0 and var modulus)
            WriteBits(0u, 8 - modulus);
    }

    /// <summary>
    /// Try to write whole bytes from the buffer to the output span.  It then advances the span
    /// past the written bytes.
    /// </summary>
    /// <param name="output">The target of the write operation.</param>
    /// <returns>True if there are no more whole bytes in the buffer to write.</returns>
    public bool TryWriteToMovingBuffer(ref Span<byte> output)
    {
        while (bitsUsed > 7 && !output.IsEmpty)
        {
            bitsUsed -= 8;
            output[0] = (byte)(buffer >> bitsUsed);
            output = output[1..];
        }
        return NothingToWrite;
    }

    /// <summary>
    /// True if there are 7 or fewer bits being in the buffer, false otherwise.
    /// </summary>
    public bool NothingToWrite => bitsUsed < 8;

    /// <summary>
    /// True if there are no bits in the buffer to be written.
    /// </summary>
    public bool IsEmpty => bitsUsed is 0;
}
