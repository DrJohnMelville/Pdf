using System;
using System.Buffers;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class FastBitmapWriterRGB8: IByteWriter
{
    public static readonly FastBitmapWriterRGB8 Instance = new();

    private FastBitmapWriterRGB8()
    {
    }

    public unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, byte* nextPos)
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