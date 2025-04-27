using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class ByteWriter8(ComponentWriter writer) : ByteWriter(ushort.MaxValue, writer)
{
    public override unsafe void WriteBytes(scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos)
    {
        while (output < nextPos && input.TryRead(out var data))
        {
            PushComponent(ref output, data);
        }
    }

    public override int MinimumInputSize => 1;
}