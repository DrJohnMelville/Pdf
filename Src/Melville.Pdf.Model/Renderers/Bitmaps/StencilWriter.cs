using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class StencilWriter : IByteWriter
{
   private readonly uint zeroColor;
   private readonly uint oneColor;
   private readonly int rowWidth;
    private int column;
    
    public StencilWriter(IReadOnlyList<double>? decode, DeviceColor color, int rowWidth)
    {
        this.rowWidth = rowWidth;
        var drawColor = color.AsPreMultiplied().AsArgbUint32();
        (zeroColor, oneColor) = ZeroIsMarkingValue(decode) ? (drawColor, 0u) : (0, drawColor);
    }

    private static bool ZeroIsMarkingValue(IReadOnlyList<double>? decode) => 
        decode == null || decode.Count< 1  || decode[0] < 0.5;
    
    public unsafe void WriteBytes(scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos)
    {
        Debug.Assert(nextPos > output);
        uint* iOutput = (uint*)output;
        uint* nextOutput = (uint*)nextPos;

        while (iOutput < nextOutput && input.TryRead(out var sourceByte))
        {
            ExpandSourceByte(ref iOutput, sourceByte);
            column %= rowWidth;
        }
        output = (byte*)iOutput;
    }

    private unsafe void ExpandSourceByte(ref uint* iOutput, byte sourceByte)
    {
        for (var currentBit = 0x80;
            currentBit > 0 && column < rowWidth;
            currentBit >>= 1, column++)
        {
            var decision = ((currentBit & sourceByte) == currentBit);
            *iOutput++ = decision ? oneColor : zeroColor;
        }
    }
}