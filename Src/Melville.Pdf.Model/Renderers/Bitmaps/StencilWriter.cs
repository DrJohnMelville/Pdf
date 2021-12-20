using System.Buffers;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class StencilWriter : IByteWriter
{
    private byte redByte;
    private byte greenByte;
    private byte blueByte;
    private bool markingValue;
    
    public StencilWriter(double[]? decode, DeviceColor color)
    {
        redByte = color.RedByte;
        blueByte = color.BlueByte;
        greenByte = color.GreenByte;
        markingValue = ZeroIsMarkingValue(decode);
    }

    private static bool ZeroIsMarkingValue(double[]? decode) => 
        decode == null || decode.Length < 1  || decode[0] < 0.5;

    public unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, byte* nextPos)
    {
        while (MoreRoomToWrite(output, nextPos) && input.TryRead(out var data))
        WriteSingleByte(ref output, nextPos, data);
    }

    private unsafe void WriteSingleByte(ref byte* output, byte* nextPos, byte data)
    {
        for (var currentBit = 0x80; currentBit > 0 && MoreRoomToWrite(output, nextPos); currentBit >>=1)
        {
            if (((currentBit & data) == 0) == markingValue)
            {
                PushPixel(ref output, redByte, greenByte, blueByte, 0xFF);
            }
            else
            {
                PushPixel(ref output, 0, 0, 0, 0);
            }
        }
    }

    private static unsafe bool MoreRoomToWrite(byte* output, byte* nextPos) => output < nextPos;

    private unsafe void PushPixel(ref byte* output, byte red, byte green, byte blue, byte alpha)
    {
        *output++ = blue;
        *output++ = green;
        *output++ = red;
        *output++ = alpha;
    }

    public int MinimumInputSize => 1;
}