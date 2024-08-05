using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.LinkedLists;

[FromConstructor]
internal partial class SingleReadStreamBuffer: StreamBackedBuffer<SingleReadStreamBuffer>
{
    public static IMultiplexSource Create(Stream source, bool leaveOpen, int bufferSize = 4096)
    {
        var linkedList = new SingleReadStreamBuffer(source, leaveOpen).With(bufferSize);
        return new MultiBufferStream(linkedList, false);
    }

    public override void HasReadTo(SequencePosition consumed)
    {
        PruneFromFront(consumed);
    }

    public override void AddReference()
    {
        base.AddReference();
        if (references > 2)
            throw new InvalidOperationException(
                "SingleReadStreamBuffer should only have one reference");
    }
}

[FromConstructor]
internal partial class MakeStreamSeekableSource : StreamBackedBuffer<MakeStreamSeekableSource>
{
    public static IMultiplexSource Create(Stream source, int bufferSize = 4096)
    {
        var linkedList = new MakeStreamSeekableSource(source, false).With(bufferSize);
        return new MultiBufferStream(linkedList, false);
    }

}
internal partial class StreamBackedBuffer<T>: LinkedList<T> where T:LinkedList<T>
{
    private bool done;

    public override LinkedList With(int blockSize)
    {
        var ret = base.With(blockSize);
        nextByteToRead = StartPosition;
        return ret;
    }

#warning need to dispose of the source and have an option to leave it open.
    [FromConstructor] private readonly Stream source;
    [FromConstructor] private bool leaveOpen;
    private LinkedListPosition nextByteToRead = default;
#warning move this up into MakeStreamSeekableSource because singlestreambuffer does not need it
    private SemaphoreSlim mutex = new(1, 1);

    public override long PrepareForRead(LinkedListPosition origin)
    {
        var desiredPosition = origin.GlobalPosition;

        mutex.Wait();
        try
        {
            while (true)
            {
                var delta = nextByteToRead.GlobalPosition - desiredPosition;
                if (done || delta > 0) return Math.Max(delta, 0);
                (nextByteToRead, done) = LoadFrom(source, nextByteToRead);
            }
        }
        finally
        {
            mutex.Release();
        }
    }

    public override async ValueTask<long> PrepareForReadAsync(
        LinkedListPosition origin)
    {
        var desiredPosition = origin.GlobalPosition;

        await mutex.WaitAsync().CA();
        try
        {
            while (true)
            {
                var delta = nextByteToRead.GlobalPosition - desiredPosition;
                if (done || delta > 0) return Math.Max(delta, 0);
                (nextByteToRead, done) = await LoadFromAsync(source, nextByteToRead).CA();
            }
        }
        finally
        {
            mutex.Release();
        }
    }

    public override LinkedListPosition FirstInvalidPosition() => nextByteToRead;

    public override bool DoneGrowing() => done;
}