
namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public readonly struct BitOffset
{
    public int ByteOffset { get;}
    public byte BitOffsetRightOfMsb {get;} // bits are numbered 0-7 MSB to LSB
    private byte BitMask {get;} // bits are numbered 0-7 MSB to LSB

    public BitOffset(int byteOffset, byte bitOffsetRightOfMsb)
    {
        ByteOffset = byteOffset;
        BitOffsetRightOfMsb = bitOffsetRightOfMsb;
        BitMask = (byte)(1 << (7-bitOffsetRightOfMsb));
    }

    public bool GetBit(byte[] buffer) => BitMask == (buffer[ByteOffset] & BitMask);

    public void WriteBit(byte[] buffer, bool value)
    {
        if (value)
            SetBit(buffer);
        else
            ClearBit(buffer);
    }

    private void SetBit(byte[] buffer) => buffer[ByteOffset] |= BitMask;
    private void ClearBit(byte[] buffer) => buffer[ByteOffset] &= (byte)~BitMask;

    public BitOffset AddRows(int lines, int stride) => 
        new((lines * stride) + ByteOffset, BitOffsetRightOfMsb);
}