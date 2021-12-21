using System.Buffers;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class StencilWriter : IByteWriter
{
    private DeviceColor color;
    private bool markingValue;
    
    public StencilWriter(double[]? decode, DeviceColor color)
    {
        this.color = color.AsPreMultiplied();
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
            BitmapPointerMath.PushPixel(ref output, 
                ShouldMarkPixel(data,currentBit)? color: DeviceColor.Invisible);
        }
    }

    private bool ShouldMarkPixel(byte data, int currentBit) => 
        ((currentBit & data) == 0) == markingValue;

    private static unsafe bool MoreRoomToWrite(byte* output, byte* nextPos) => output < nextPos;

    public int MinimumInputSize => 1;
}