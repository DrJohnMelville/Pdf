using System;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using SixLabors.ImageSharp.Processing;

namespace Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

public class BitWriter
{
    private byte residue;
    private byte spotsAvailable;
        
    public BitWriter()
    {
        residue = 0;
        spotsAvailable = 8;
    }

    public int WriteBits(uint data, int bits, in Span<byte> target) =>
        WriteBits((int)data, bits, target);
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
    public int FinishWrite(in Span<byte> target) => 
        NoBitsWaitingToBeWritten() ? 0 : WriteCurrentByte(target);
    public (byte, byte) GetState() => (residue, spotsAvailable);
    public void SetState((byte, byte) state) => (residue, spotsAvailable) = state;

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

    public (int bitsRead, int bytesWritten) WriteWholeBitSpan(
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

public readonly struct BitToByte
{
    private readonly byte trueVal;
    private readonly byte falseVal;

    public BitToByte(byte trueVal, byte falseVal)
    {
        this.trueVal = trueVal;
        this.falseVal = falseVal;
    }

    public byte ByteForBit(bool value) => value ? trueVal : falseVal;
} 