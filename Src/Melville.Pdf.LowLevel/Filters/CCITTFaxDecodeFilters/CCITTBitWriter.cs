using System;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public ref struct CcittBitWriter
{
    private readonly BitWriter writer = new();
    private BitTarget target;

    public CcittBitWriter(in Span<byte> destination)
    {
        target = new BitTarget(destination, writer);
    }

    public void WritePass() => WriteBits(0b0001, 4);

    public void WriteVertical(int delta)

    {
        switch (delta)
        {
            case -3: WriteBits(0b0000010,7); break;
            case -2: WriteBits(0b000010,6); break;
            case -1: WriteBits(0b010,3); break;
            case  1: WriteBits(0b011,3); break;
            case  2: WriteBits(0b000011,6); break;
            case  3: WriteBits(0b0000011,7); break;
            case  0: WriteBits(0b1,1); break;
            default: throw new InvalidOperationException("Invalid Vertical Offset");
        }    
    }

    public bool WriteHorizontal(bool firstIsWhite, int firstRun, int secondRun)
    {
        var savedState = writer.GetState();
        var savedTarget = target;
        if (HorizontalSpanEncoder.Write(ref target, firstIsWhite, firstRun, secondRun)) return true;
        writer.SetState(savedState);
        target = savedTarget;
        return false;
    }

    private void WriteBits(uint data, int bits)
    {
        Debug.Assert(HasRoomToWrite());
        target.TryWriteBits(data, bits);
    }

    public bool HasRoomToWrite() => target.BytesRemaining() > 0;

    public Span<byte> WrittenSpan()
    {
        Debug.Assert(HasRoomToWrite());
        target.FinishWrite();
        return target.WrittenSpan();
    }
}