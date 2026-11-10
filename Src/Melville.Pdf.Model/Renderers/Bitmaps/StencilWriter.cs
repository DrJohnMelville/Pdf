using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class StencilWriter : IByteWriter
{
   private readonly DeviceColor zeroColor;
   private readonly DeviceColor oneColor;
   private readonly int rowWidth;
    private int column;

    private static DeviceColor DefaultColor = new DeviceColor();
    
    public StencilWriter(IReadOnlyList<double>? decode, DeviceColor color, int rowWidth)
    {
        this.rowWidth = rowWidth;
        var drawColor = color.AsPreMultiplied();
        (zeroColor, oneColor) = ZeroIsMarkingValue(decode) ? (drawColor, DefaultColor) : (DefaultColor, drawColor);
    }

    private static bool ZeroIsMarkingValue(IReadOnlyList<double>? decode) => 
        decode == null || decode.Count< 1  || decode[0] < 0.5;
    
    public unsafe void WriteBytes(scoped ref SpanParsingBuffer input, ref Span<DeviceColor> output)
    {

        while (!output.IsEmpty && input.TryGetUInt8(out var sourceByte))
        {
            ExpandSourceByte(ref output, sourceByte);
            column %= rowWidth;
        }
    }

    private unsafe void ExpandSourceByte(ref Span<DeviceColor> output, byte sourceByte)
    {
        for (var currentBit = 0x80;
            currentBit > 0 && column < rowWidth;
            currentBit >>= 1, column++)
        {
            var decision = ((currentBit & sourceByte) == currentBit);
            output[0] = decision ? oneColor : zeroColor;
            output = output[1..];
        }
    }
}