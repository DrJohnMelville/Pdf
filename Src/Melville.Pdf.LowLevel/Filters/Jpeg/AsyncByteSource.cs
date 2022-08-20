using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;


public interface IAsyncByteSource
{
    ValueTask Initialize();
    ValueTask<byte> GetByte();
}

public partial class AsyncByteSource: IAsyncByteSource
{
    [FromConstructor] private readonly PipeReader source;
    private ReadOnlySequence<byte> sequence;
    private SequencePosition sequencePosition;
    private bool atEndOfStream;

    public async ValueTask Initialize() => SetNewSequence(await source.ReadAsync().CA());

    private void SetNewSequence(in ReadResult readResult)
    {
        sequence = readResult.Buffer;
        sequencePosition = sequence.Start;
        atEndOfStream = readResult.IsCompleted;
    }


    public async ValueTask<byte> GetByte()
    {
        if (NeedMoreBytes())
        {
            source.AdvanceTo(sequencePosition, sequence.End);
            SetNewSequence(await source.ReadAsync().CA());
        }
        return GetByteFromSequence();
    }
    
    private byte GetByteFromSequence()
    {
        if (BytesRemainingInSequence() == 0) return 0;
        var currentByte = ByteAtCurrentPosition();
        IncrementSequencePosition();
        return currentByte;
    }


    private byte ByteAtCurrentPosition() => sequence.Slice(sequencePosition).FirstSpan[0];

    private void IncrementSequencePosition() => 
        sequencePosition = sequence.GetPosition(1, sequencePosition);

    private bool NeedMoreBytes() => BytesRemainingInSequence() < 1 && !atEndOfStream;

    private long BytesRemainingInSequence() => sequence.Slice(sequencePosition).Length;
}