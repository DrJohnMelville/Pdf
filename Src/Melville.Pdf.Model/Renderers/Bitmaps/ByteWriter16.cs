using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class ByteWriter16(ComponentWriter writer) : ByteWriter(ushort.MaxValue, writer)
{
    public override unsafe void WriteBytes(
        scoped ref SpanParsingBuffer input, ref Span<DeviceColor> output)
    {
        while (!output.IsEmpty && input.CurrentLength >= 2)
        {
            PushComponent(ref output, input.GetUInt16BigEndian());
        }
    }
    private static int UIntFromBytes(byte high, byte low) => (high << 8)|low;
}