using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Melville.Parsing.SequenceReaders;
using Melville.Parsing.Streams.Bases;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter;

internal sealed class InvertingMemoryStream : DefaultBaseStream, ISequenceSource
{
    private Memory<byte> data;

    public InvertingMemoryStream(byte[] data, int length) : base(true, false, false)
    {
        this.data = data.AsMemory(0, length);
        InvertSpan(this.data.Span);
    }

    void InvertSpan(Span<byte> span)
    {
        var remainder = span.Length % 32;
        if (remainder is > 0) InvertBytes(span[..remainder]);
        InvertBytesFast(span[remainder..]);
    }

    private static void InvertBytesFast(Span<byte> span)
    {
        var bigSpan = MemoryMarshal.Cast<byte, Vector256<byte>>(span);
        for (int i = 0; i < bigSpan.Length; i++)
        {
            bigSpan[i] = ~bigSpan[i];
        }
    }

    private static void InvertBytes(Span<byte> asSpan)
    {
        for (int i = 0; i < asSpan.Length; i++)
        {
            asSpan[i] = (byte)~asSpan[i];
        }
    }

    public override int Read(Span<byte> buffer)
    {
        var source = data.Span.TakeMax(buffer.Length, out var actualLength);
        source.CopyTo(buffer);
        source = source[actualLength..];
        return actualLength;
    }

    public ReadOnlySequence<byte> AsSequence() => new ReadOnlySequence<byte>(data);
}