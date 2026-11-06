using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.SegmentParsers;

internal readonly struct ReadUnknownSegmentLengthOld(PipeReader reader)
{

   const int regionHeaderLength = 18;
    private static ReadOnlySpan<byte> MmrEndSequence => [0x00, 0x00]; 
    private static ReadOnlySpan<byte> ArithEndSequence => [0xFF, 0xAC];
    private static ReadOnlySpan<byte> SelectEndSequence(bool isMMR) => isMMR ? MmrEndSequence : ArithEndSequence;

    public async ValueTask<(ReadOnlySequence<byte> Seq, bool shoudAdvance)> ReadSegmentAsync()
    {

        var initial = await reader.ReadAtLeastAsync(regionHeaderLength).CA();
        ReadHeightAndType(initial.Buffer, out var height, out var isMMR);
        reader.AdvanceTo(initial.Buffer.Start, initial.Buffer.GetPosition(regionHeaderLength));
        return EnsureRowsConsistent(await FindEndPositionAsync(isMMR).CA(), height);
    }

    private void ReadHeightAndType(ReadOnlySequence<byte> buffer, out uint height, out bool isMMR)
    {
        var reader = new SequenceReader<byte>(buffer);
        reader.Advance(4);
        reader.TryGetUInt32BigEndian(out height);
        reader.Advance(8);
        reader.TryRead(out byte flag);
        isMMR = (flag & 0x01) == 0x01;
    }

    private async ValueTask<ReadOnlySequence<byte>> FindEndPositionAsync(bool isMmr)
    {
        while (true)
        {
            var result = await reader.ReadAsync().CA();
            if (SearchSequenceFor(result.Buffer.Slice(regionHeaderLength-1), SelectEndSequence(isMmr), out var endpoint))
                return result.Buffer.Slice(0, endpoint);
            if (result.IsCompleted) 
                throw new InvalidDataException("End of segment not found");
            reader.AdvanceTo(result.Buffer.Start, endpoint);
        }
    }

    private bool SearchSequenceFor(ReadOnlySequence<byte> source, ReadOnlySpan<byte> endSeq, out SequencePosition sequencePosition)
    {
        var reader = new SequenceReader<byte>(source);
        sequencePosition = source.End;
        while (reader.UnreadSequence.Length >= 0)
        {
            if (!reader.TryAdvanceTo(endSeq[0], true)) return false;
            if (!reader.TryPeek(out byte next)) return false;
            if (next != endSeq[1]) continue;
            break;
        }

        if (!reader.TryAdvance(5)) return false; // 1 byte for the end of the tag and 4 bytes for number of rows.
        sequencePosition = reader.Position;
        return true;
    }

    // As noted in sections 7.4.6.4 and 7.2.7 of the JBIG spec a region with an unknown length can declare one row count
    // in the segment header, and then declare a different, and less row count immediately after the section data.
    // this is a rare corner case, so to handle it correctly we just allocate an array for the entire region and fixup the
    // header so that the region parser will only see regions with consistent row fields.
    private (ReadOnlySequence<byte> Seq, bool shoudAdvance) EnsureRowsConsistent(ReadOnlySequence<byte> source,
        uint declaredHeight)
    {
        if (HasValidRowField(source, declaredHeight)) return (source, true);
        
        var ret = source.ToArray();
        reader.AdvanceTo(source.End);

        CopyEndRowValueToHeader(ret);
        return (new ReadOnlySequence<byte>(ret), false);
    }

    private static void CopyEndRowValueToHeader(byte[] ret)
    {
        ret[4] = ret[^4];
        ret[5] = ret[^3];
        ret[6] = ret[^2];
        ret[7] = ret[^1];
    }

    private static bool HasValidRowField(ReadOnlySequence<byte> source, uint declaredHeight)
    {
        var r2 = new SequenceReader<byte>(source.Slice(source.Length - 4));
        return declaredHeight == r2.GetUInt32BigEndian();
    }
}

internal readonly struct ReadUnknownSegmentLength(ParsingBuffer reader)
{

    const int regionHeaderLength = 18;
    private static ReadOnlySpan<byte> MmrEndSequence => [0x00, 0x00];
    private static ReadOnlySpan<byte> ArithEndSequence => [0xFF, 0xAC];
    private static ReadOnlySpan<byte> SelectEndSequence(bool isMMR) => isMMR ? MmrEndSequence : ArithEndSequence;

#warning I sholud be able to use TakeBytes to get rid of the shouldAdvance
    public async ValueTask<(ReadOnlySequence<byte> Seq, bool shoudAdvance)> ReadSegmentAsync()
    {
        await reader.EnsureBytesAsync(regionHeaderLength);
        var endPoint = await FindEndPositionAsync(IsMMRSegment());
        Memory<byte> memory = reader.PeekMemory(endPoint).ToArray();
        EnsureRowsConsistent(memory.Span);
#warning get rid of inefficency once the parsing buffer lasts long enough
        reader.Advance(memory.Length);
        return (new ReadOnlySequence<byte>(memory), false);
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