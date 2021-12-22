using System;
using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class ByteWriter16 : ByteWriter
{
    public ByteWriter16(IComponentWriter writer) :
        base(UInt16.MaxValue, writer)
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