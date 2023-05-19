using System;
using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class ByteWriter16 : ByteWriter
{
    public ByteWriter16(ComponentWriter writer) :
        base(ushort.MaxValue, writer)
    {
    }

    public override unsafe void WriteBytes(scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos)
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