using System;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Filters.LzwFilter;

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
}