using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class ByteWriter8(ComponentWriter writer) : ByteWriter(ushort.MaxValue, writer)
{
    public override unsafe void WriteBytes(
        scoped ref SpanParsingBuffer input, ref Span<DeviceColor> output)
    {
        while (!output.IsEmpty && input.TryGetUInt8(out var data))
        {
            PushComponent(ref output, data);
        }
    }
}