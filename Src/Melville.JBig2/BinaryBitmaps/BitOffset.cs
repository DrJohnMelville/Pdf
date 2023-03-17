
namespace Melville.JBig2.BinaryBitmaps;

/// <summary>
/// Returns a bit offset into an array of bits packed into bytes.
/// </summary>
public readonly struct BitOffset
{
    /// <summary>
    /// Number of bytes represented by this offset
    /// </summary>
    public int ByteOffset { get;}
    /// <summary>
    /// number of additional bits represented by this offset
    /// </summary>
    public byte BitOffsetRightOfMsb {get;} // bits are numbered 0-7 MSB to LSB
    private byte BitMask {get;} // bits are numbered 0-7 MSB to LSB

    /// <summary>
    /// Create a new bit offset
    /// </summary>
    /// <param name="byteOffset">Whole bytes in the offset</param>
    /// <param name="bitOffsetRightOfMsb">Additional bits in the offset</param>
    public BitOffset(int byteOffset, byte bitOffsetRightOfMsb)
    {
        ByteOffset = byteOffset;
        BitOffsetRightOfMsb = bitOffsetRightOfMsb;
        BitMask = (byte)(1 << (7-bitOffsetRightOfMsb));
    }

    /// <summary>
    /// Retrieve the bit at this offset in the given buffer
    /// </summary>
    /// <param name="buffer">A buffer to offset into</param>
    /// <returns>A bit at this offset's location in the given buffer</returns>
    public bool GetBit(byte[] buffer) => BitMask == (buffer[ByteOffset] & BitMask);

    /// <summary>
    /// Write a boolean value to the given buffer at this offset's location
    /// </summary>
    /// <param name="buffer">The buffer to write to</param>
    /// <param name="value">The binary value to write</param>
    public void WriteBit(byte[] buffer, bool value)
    {
        if (value)
            SetBit(buffer);
        else
            ClearBit(buffer);
    }

    private void SetBit(byte[] buffer) => buffer[ByteOffset] |= BitMask;
    private void ClearBit(byte[] buffer) => buffer[ByteOffset] &= (byte)~BitMask;

    /// <summary>
    /// Add a set number of rows to the location of this offset
    /// </summary>
    /// <param name="lines">The number of lines to add</param>
    /// <param name="stride">The stride of a singele line</param>
    /// <returns></returns>
    public BitOffset AddRows(int lines, int stride) => 
        new((lines * stride) + ByteOffset, BitOffsetRightOfMsb);
}