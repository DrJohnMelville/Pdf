using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal readonly struct ByteBuffer16
{
    private readonly byte[] byteBuffer = new byte[16];

    public ByteBuffer16()
    {
    }

    public byte[] AsArray(in Span<byte> input)
    {
        Debug.Assert(input.Length == 16);
        input.CopyTo(byteBuffer);
        return byteBuffer;
    }
}