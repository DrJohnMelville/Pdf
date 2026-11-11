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

#warning I sholud be able to use TakeBytes to get rid of the shouldAdvance
    public async ValueTask<Memory<byte>> ReadSegmentAsync()
    {
        await reader.EnsureBytesAsync(regionHeaderLength);
        var endPoint = await FindEndPositionAsync(IsMMRSegment());
        Memory<byte> memory = await reader.TakeBytesAsync(endPoint).CA();
        EnsureRowsConsistent(memory.Span);
        return memory;
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

    private void EnsureRowsConsistent(Span<byte> ret) => ret[^4..].CopyTo(ret[4..]);
}