using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.SegmentParsers;

internal readonly struct ReadUnknownSegmentLength(ParsingBuffer reader)
{

    const int regionHeaderLength = 18;
    private static ReadOnlySpan<byte> MmrEndSequence => [0x00, 0x00];
    private static ReadOnlySpan<byte> ArithEndSequence => [0xFF, 0xAC];
    private static ReadOnlySpan<byte> SelectEndSequence(bool isMMR) => isMMR ? MmrEndSequence : ArithEndSequence;

    public async ValueTask<ReadOnlyMemory<byte>> ReadSegmentAsync()
    {
#warning need to handle inconsistentSegmentLengths
        await reader.EnsureBytesAsync(regionHeaderLength);
        var endPoint = await FindEndPositionAsync(IsMMRSegment());
        var memory = await reader.TakeBytesAsync(endPoint).CA();
        return VerifyConsistent(memory);
    }

    private static ReadOnlyMemory<byte> VerifyConsistent(ReadOnlyMemory<byte> memory)
    {
        var span = memory.Span;
        if (span[^4..].SequenceEqual(span[4..8])) return memory;
        // this is an allocation but it is a rare corner case.
        var buffer = new byte[span.Length];
        span.CopyTo(buffer);
        var targetSpan = buffer.AsSpan();
        targetSpan[^4..].CopyTo(targetSpan[4..]);
        return buffer;
 }

    private bool IsMMRSegment() => (reader.Peek()[16] & 0x01) == 0x01;

    private async ValueTask<int> FindEndPositionAsync(bool isMmr)
    {
        var endSeq = SelectEndSequence(isMmr);
        var endPos = -1;
        while ((endPos = reader.Peek().IndexOf(endSeq)) is < 0)
        {
            if (reader.DoneReadingSource)
                throw new InvalidOperationException("Could Not find end of segment");
            await reader.ReadBytesAsync();
        }
        return endPos + 6; // 2 byte for end of the tag and 4 bytes for the length.
    }
}