using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class StencilWriter : IByteWriter
{
   private readonly uint zeroColor;
    private readonly uint oneColor;
    
    public StencilWriter(double[]? decode, DeviceColor color)
    {
        var drawColor = color.AsPreMultiplied().AsArgbUint32();
        (zeroColor, oneColor) = ZeroIsMarkingValue(decode) ? (drawColor, 0u) : (0, drawColor);
    }

    private static bool ZeroIsMarkingValue(double[]? decode) => 
        decode == null || decode.Length < 1  || decode[0] < 0.5;
    
    public unsafe void WriteBytes(scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos)
    {
        Debug.Assert(nextPos > output);
        uint* iOutput = (uint*)output;
        uint* nextOutput = (uint*)nextPos;
        
        var (sourceBytesInDestination, rem) = Math.DivRem((nextOutput - iOutput)-0, 8);
        var sourceBytesToRead = Math.Min(sourceBytesInDestination, input.Remaining);

        for (int i = 0; i < sourceBytesToRead; i++)
        {
            input.TryRead(out var data);
            ExpandSourceByte(ref iOutput, data, 0);
        }
        if (rem > 0 && input.TryRead(out var data1))
        {
            ExpandSourceByte(ref iOutput, data1, 0x80 >> (int)rem);
        }

        output = (byte*)iOutput;
    }

    private unsafe void ExpandSourceByte(ref uint* iOutput, byte sourceByte, int stop)
    {
        for (var currentBit = 0x80; currentBit > stop; currentBit >>= 1)
        {
            var decision = ((currentBit & sourceByte) == currentBit);
            *iOutput++ = decision ? oneColor : zeroColor;
        }
    }


    public int MinimumInputSize => 1;
}