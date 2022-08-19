using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

// this class reads bits asynchronously from a pipereader, but it is unique to the JPEG reader because
// it also handles the bit unpacking specificall if 0x00 is followed by 0xFF the ox00 is ifnored
public sealed partial class AsyncBitSource
{
    public static async ValueTask<AsyncBitSource> Create(PipeReader source)
    {
        var ret = new AsyncBitSource(source);
        ret.SetNewSequence(await source.ReadAsync().CA());
        return ret;
    }

    [FromConstructor] private readonly PipeReader source;
    private ReadOnlySequence<byte> sequence = default;
    private SequencePosition sequencePosition = default;
    private byte currentbyte;
    private int bitSelector = 0;
    private bool atEndOfStream;


    private void SetNewSequence(in ReadResult readResult)
    {
        sequence = readResult.Buffer;
        sequencePosition = sequence.Start;
        atEndOfStream = readResult.IsCompleted;
    }

    public async ValueTask<byte> ReadBitAsync()
    {
        if (NoMoreBitsInByte())
        {
            while (NeedMoreBytes())
            {
                source.AdvanceTo(sequencePosition, sequence.End);
                var ret = await source.ReadAsync().CA();
                SetNewSequence(ret);
            }
            GetByteFromSequence();
        }

        return GetBitFromByte();
    }

    private byte GetBitFromByte()
    {
        var ret = (currentbyte & bitSelector) == 0 ? (byte)0 : (byte)1;
        bitSelector >>= 1;
        return ret;
    }

    private void GetByteFromSequence()
    {
        currentbyte = ByteAtCurrentPosition();
        IncrementSequencePosition();
        TryStripStuffedByte();
        bitSelector = 0x80;
    }

    private void TryStripStuffedByte()
    {
        if (!CurrentIsStuffedByte()) return;
        IncrementSequencePosition();
        currentbyte = 0xFF;
    }

    private bool CurrentIsStuffedByte()
    {
        if (currentbyte != 0) return false;
        var span = sequence.Slice(sequencePosition).FirstSpan;
        return (span.Length > 0) && span[0] == 0xFF;
    }

    private byte ByteAtCurrentPosition() => sequence.Slice(sequencePosition).FirstSpan[0];

    private void IncrementSequencePosition() => 
        sequencePosition = sequence.GetPosition(1, sequencePosition);

    // we get more bytes when we only have 1 byte left, because if that byte is zero,
    // then we need to be able to peek at the next byte to know how to process it.
    private bool NeedMoreBytes() => sequence.Slice(sequencePosition).Length < 2 && !atEndOfStream;

    private bool NoMoreBitsInByte() => bitSelector == 0;
}