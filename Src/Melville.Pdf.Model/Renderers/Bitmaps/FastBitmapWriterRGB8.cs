using System;
using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class FastBitmapWriterRGB8: IByteWriter
{
    public unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, byte* nextPos)
    {
        while (input.Remaining >= 3 && output < nextPos)
        {
            var segment = new Span<byte>(output, 4);
            input.TryRead(out *(output + 2));
            input.TryRead(out *(output + 1));
            input.TryRead(out *(output));
            output += 3;
            *output++ = 0xFF;
        }
    }
    public int MinimumInputSize => 3;
}