using System.Buffers;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class ByteWriter16 : ByteWriter
{
    private const float MaxValue = (1 << 16) - 1;
    public ByteWriter16(IColorSpace colorSpace) : base(colorSpace)
    {
    }

    public override unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, byte* nextPos)
    {
        while (output < nextPos && input.TryPeek(out var high) && input.TryPeek(1, out var low))
        {
            input.Advance(2);
            PushComponent(ref output, ((high << 8)|low), ushort.MaxValue);
        }
    }

    public override int MinimumInputSize => 2;
}