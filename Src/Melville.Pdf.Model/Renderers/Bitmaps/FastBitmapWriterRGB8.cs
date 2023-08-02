using System.Buffers;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

[StaticSingleton]
internal partial class FastBitmapWriterRGB8: IByteWriter
{
    public unsafe void WriteBytes(scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos)
    {
        while (input.Remaining >= 3 && output < nextPos)
        {
            input.TryRead(out output[2]);
            input.TryRead(out output[1]);
            input.TryRead(out *(output));
            output[3] = 0xFF;
            output += 4;
        }
    }
    
    public int MinimumInputSize => 3;
}