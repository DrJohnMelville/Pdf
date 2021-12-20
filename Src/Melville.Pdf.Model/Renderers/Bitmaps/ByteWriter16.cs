using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class ByteWriter16 : ByteWriter
{
    public ByteWriter16(IColorSpace colorSpace, ClosedInterval[] outputIntervals) :
        base(colorSpace, outputIntervals, UInt16.MaxValue)
    {
    }

    public override unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, byte* nextPos)
    {
        while (output < nextPos && input.TryPeek(out var high) && input.TryPeek(1, out var low))
        {
            input.Advance(2);
            PushComponent(ref output, UIntFromBytes(high, low));
        }
    }

    private static int UIntFromBytes(byte high, byte low) => (high << 8)|low;

    public override int MinimumInputSize => 2;
}