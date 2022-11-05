using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

internal readonly struct ByteBuffer
{
    private readonly byte[] byteBuffer;
    public ByteBuffer(int length)
    {
        byteBuffer = new byte[length];
    }

    public byte[] AsArray(in Span<byte> input)
    {
        Debug.Assert(input.Length == byteBuffer.Length);
        input.CopyTo(byteBuffer);
        return byteBuffer;
    }
}