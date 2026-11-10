using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class NBitByteWriter(ComponentWriter writer, int bits, int lineWidth) : 
    ByteWriter(MaxValueForBits(bits), writer)
{
    private static byte MaxValueForBits(int bits) => (byte)((1 << bits) - 1);
    int currentCol = 0;

    public override unsafe void WriteBytes(
        scoped ref SpanParsingBuffer input, ref Span<DeviceColor> output)
    {
        while (!output.IsEmpty && input.TryGetUInt8(out var readVal))
        {
            PushSingleByte(ref output, readVal);
            currentCol %= lineWidth;
        }
    }

    private unsafe void PushSingleByte(ref Span<DeviceColor> output, byte readVal)
    {
        for (var bitsLeft = 8;  
            bitsLeft > 0 && !output.IsEmpty && currentCol < lineWidth;
            bitsLeft -= bits, currentCol++)
        {
            PushComponent(ref output, (readVal >> (bitsLeft - bits)) & MaxValue);
        }
    }
}