using Melville.INPC;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Melville.Pdf.Model.Renderers.Bitmaps;


[StaticSingleton]
internal partial class FastBitmapWriterRGB8 : IByteWriter
{
    public void WriteBytes(
        scoped ref SpanParsingBuffer input, ref Span<DeviceColor> output, bool isFinal)
    {
        var bytes = MemoryMarshal.AsBytes(output);
        try
        {
            if (Vector512.IsHardwareAccelerated)
            {
                VectorizedBitmapWriter512.WriteBytes(ref input, ref bytes);
                if (!isFinal) return;
            }
            else if (Vector256.IsHardwareAccelerated)
            {
                VectorizedBitmapWriter256.WriteBytes(ref input, ref bytes);
                if (!isFinal) return;
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                VectorizedBitmapWriter128.WriteBytes(ref input, ref bytes);
                if (!isFinal) return;
            }
            else if (Vector64.IsHardwareAccelerated)
            {
                VectorizedBitmapWriter64.WriteBytes(ref input, ref bytes);
                if (!isFinal) return;
            }
            WriteBytes(ref input, ref bytes);
        }
        finally
        {
            output = MemoryMarshal.Cast<byte, DeviceColor>(bytes);
        }
    }
    public void WriteBytes(
        ref SpanParsingBuffer input, ref Span<byte> output)
    {
        while (input.CurrentLength >= 3 && output.Length >= 4)
        {
            var inputSpan = input.Peek();
            output[2] = inputSpan[0];
            output[1] = inputSpan[1];
            output[0] = inputSpan[2];
            output[3] = 0xFF;
            input.Advance(3);
            output = output[4..];
        }
    }
}
