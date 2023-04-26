using System.Diagnostics;

namespace Melville.Parsing.VariableBitEncoding;

/// <summary>
/// Write numbers with a variable number of bits.
/// </summary>
public class BitWriter
{
    private byte residue;
    private byte spotsAvailable;
        
    /// <summary>
    /// Create a new BitWriter
    /// </summary>
    public BitWriter()
    {
        residue = 0;
        spotsAvailable = 8;
    }

    /// <summary>
    /// Write a given number to the target span with a given number of bits.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <param name="bits">Number of bits used to encode the data.</param>
    /// <param name="target">The span to write to.</param>
    /// <returns>Number of bytes written.</returns>
    public int WriteBits(uint data, int bits, in Span<byte> target) =>
        WriteBits((int)data, bits, target);

    /// <summary>
    /// Write a given number to the target span with a given number of bits.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <param name="bits">Number of bits used to encode the data.</param>
    /// <param name="target">The span to write to.</param>
    /// <returns>Number of bytes written.</returns>
    public int WriteBits(int data, int bits, in Span<byte> target)
    {
        var position = 0;
        if (spotsAvailable == 0)
        {
            position +=  WriteCurrentByte(target);
        }
        int leftOverBits = bits - spotsAvailable;
        if (leftOverBits <= 0)
        {
            AddBottomBitsToResidue(data, bits);
            return position;
        }

        AddBottomBitsToResidue(data >> leftOverBits, spotsAvailable);
        return position + WriteBits(data, leftOverBits, target.Slice(position));
    }

    private void AddBottomBitsToResidue(int data, int bits)
    {
        Debug.Assert(bits <= spotsAvailable);
        var spotsAvailableAfterBitsAdded = (spotsAvailable - bits);
        residue |= (byte) ((data & BitUtilities.Mask(bits)) << spotsAvailableAfterBitsAdded);
        spotsAvailable = (byte) spotsAvailableAfterBitsAdded;
    }
    
    /// <summary>
    /// Determines whether there is unwritten data in the internal buffer.
    /// </summary>
    /// <returns>True if there is no unwritten data in the internal buffer, false otherwise.</returns>
    public bool NoBitsWaitingToBeWritten() => spotsAvailable > 7;

    private int WriteCurrentByte(in Span<byte> target)
    {
        WriteByte(target);
        return  1;
    }

    private void WriteByte(in Span<byte> span)
    {
        span[0] = residue;
        residue = 0;
        spotsAvailable = 8;
    }

    /// <summary>
    /// Write any unwritten bits to the target.
    /// </summary>
    /// <param name="target">The span that re ceives the output data</param>
    /// <returns>The number of bits written to the target.</returns>
    public int FinishWrite(in Span<byte> target) => 
        NoBitsWaitingToBeWritten() ? 0 : WriteCurrentByte(target);

    /// <summary>
    /// Get a memento of the state of this object.
    /// </summary>
    /// <returns>A tuple with the current state of the object.</returns>
    public (byte, byte) GetState() => (residue, spotsAvailable);

    /// <summary>
    /// Restore the object from a memento created by GetState
    /// </summary>
    /// <param name="state">The memento to restore from.</param>
    public void SetState((byte, byte) state) => (residue, spotsAvailable) = state;

    /// <summary>
    /// Write a span of booleans to a byte span, packed in to bytes.
    /// </summary>
    /// <param name="bits">Span of bools to write.</param>
    /// <param name="bytes">The span of bytes to write</param>
    /// <param name="bitToByte">A span that dictates whether true maps to one or 0.</param>
    /// <returns>Number of bytes written to the output span.</returns>
    public (int bitsRead, int bytesWritten) WriteBitSpan(
        in ReadOnlySpan<bool> bits, in Span<byte> bytes, in BitToByte bitToByte)
    {
        if (NoBitsWaitingToBeWritten()) return WriteWholeBitSpan(bits, bytes, bitToByte);
        if (bytes.Length < 1) return (0, 0);

        int bitPefix = Math.Min(spotsAvailable, bits.Length);
        
        for (int i = 0; i < bitPefix; i++)
        {
            WriteBits(bitToByte.ByteForBit(bits[i]), 1, bytes);
        }

        var (innerRead, innerWritten) = WriteWholeBitSpan(bits[bitPefix..], bytes[1..], bitToByte);
        return (bitPefix+innerRead, 1+innerWritten);
    }

    private (int bitsRead, int bytesWritten) WriteWholeBitSpan(
        in ReadOnlySpan<bool> bits, in Span<byte> bytes, in BitToByte bitToByte)
    {
        Debug.Assert(NoBitsWaitingToBeWritten());
        var wholeBytes = Math.Min(bits.Length / 8, bytes.Length);
        
        Copy8BitGroups(bits,  bytes, bitToByte, wholeBytes);
        var read = 8 * wholeBytes;

        if (wholeBytes < bytes.Length && read < bits.Length)
        {
            Debug.Assert(NoBitsWaitingToBeWritten());
            Debug.Assert(bits.Length - read < 8);
            // this will never actually write - just bufer the leftover bits
            while (read < bits.Length) WriteBits(bitToByte.ByteForBit(bits[read++]), 1, bytes);
        }

        return (read, wholeBytes);
    }

    private static unsafe void Copy8BitGroups(in ReadOnlySpan<bool> bits, in Span<byte> bytes, in BitToByte bitToByte,
        int wholeBytes)
    {
        fixed (bool* source = bits)
        {
            fixed (byte* destOrigin = bytes)
            {
                Copy8BitGroups(source, destOrigin, destOrigin+wholeBytes, bitToByte);
            }
        }
    }

    private static unsafe void Copy8BitGroups(bool* source, byte* destOrigin, byte* destEnd, in BitToByte bitToByte)
    {
        for (var dest = destOrigin; dest < destEnd; dest++)
        {
              *(dest) = (byte)(
                (bitToByte.ByteForBit(*(source++)) << 7) |
                (bitToByte.ByteForBit(*(source++)) << 6) |
                (bitToByte.ByteForBit(*(source++)) << 5) |
                (bitToByte.ByteForBit(*(source++)) << 4) |
                (bitToByte.ByteForBit(*(source++)) << 3) |
                (bitToByte.ByteForBit(*(source++)) << 2) |
                (bitToByte.ByteForBit(*(source++)) << 1) |
                (bitToByte.ByteForBit(*(source++))));
        }
    }
}

/// <summary>
/// Represents the mapping ot booleans to bits.
/// </summary>
public readonly struct BitToByte
{
    private readonly byte trueVal;
    private readonly byte falseVal;

    /// <summary>
    /// Create a BitToByte
    /// </summary>
    /// <param name="trueVal">Byte that represents true.</param>
    /// <param name="falseVal">Byte that represents false.</param>
    public BitToByte(byte trueVal, byte falseVal)
    {
        this.trueVal = trueVal;
        this.falseVal = falseVal;
    }

    /// <summary>
    /// Return the byte for a true or false bit.
    /// </summary>
    /// <param name="value">The bit to encode.</param>
    /// <returns>A byte that represents a given bits.</returns>
    public byte ByteForBit(bool value) => value ? trueVal : falseVal;
} 