using System;
using System.Buffers;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;


namespace Melville.Parsing.LinkedLists;


internal abstract class LinkedList: CountedMultiplexSource
{
    public static readonly LinkedList Empty = CreateEmpty();

    private static LinkedList CreateEmpty()
    {
        var ret = new MultiBufferStreamList();
        ret.endPosition = ret.StartPosition = LinkedListPosition.NullPosition;
        return ret;
    }

    public LinkedListPosition StartPosition { get; private set; }
    private LinkedListPosition endPosition;
    public LinkedListPosition EndPosition => endPosition;
    private int blockSize;

    protected internal LinkedList()
    {
    }


    public virtual LinkedList With(int blockSize)
    {
        ResetState();
        this.blockSize = blockSize;
        LinkedListNode firstNode = CreateNewBlock();
        StartPosition = endPosition = new LinkedListPosition(firstNode, 0);
        return this;
    }

    public LinkedList With(ReadOnlyMemory<byte> source)
    {
        ResetState();
        LinkedListNode firstNode = LinkedListNode.Rent(source);
        blockSize = 0;
        StartPosition = new LinkedListPosition(firstNode, 0);
        endPosition = new LinkedListPosition(firstNode, source.Length);
        return this;
    }

    private LinkedListNode CreateNewBlock() => LinkedListNode.Rent(blockSize);

    public ReadOnlySequence<byte> AsSequence() => SequenceAfter(StartPosition);

    public ReadOnlySequence<byte> SequenceAfter(LinkedListPosition start) =>
        start.SequenceTo(EndPosition);

    public ReadOnlySequence<byte> ValidSequence(LinkedListPosition start) =>
        start.SequenceTo(FirstInvalidPosition());

    public virtual LinkedListPosition FirstInvalidPosition() => EndPosition;

    public (int, LinkedListPosition) Read(LinkedListPosition origin, Span<byte> buffer) => 
        ReadCore(origin, buffer, PrepareForRead(origin));

    public virtual long PrepareForRead(LinkedListPosition origin) => 
        endPosition.GlobalPosition - origin.GlobalPosition;

    public async ValueTask<(int, LinkedListPosition)> ReadAsync(
        LinkedListPosition origin, Memory<byte> buffer)
    {
        var bytesToRead = await PrepareForReadAsync(origin).CA();
        return ReadCore(origin, buffer.Span, bytesToRead);
    }

    public virtual ValueTask<long> PrepareForReadAsync(LinkedListPosition origin) => 
        new(endPosition.GlobalPosition - origin.GlobalPosition);

    private (int, LinkedListPosition) ReadCore(LinkedListPosition origin, Span<byte> buffer, long availableBytes)
    {
        var length = (int )Math.Min(availableBytes, buffer.Length);
        var seq = SequenceAfter(origin);
        var reader = new SequenceReader<byte>(seq);
        reader.TryCopyTo(buffer[..length]);
        return (length, seq.GetPosition(length));
    }

    public LinkedListPosition Write(
        LinkedListPosition currentPosition, ReadOnlySpan<byte> buffer) =>
        UpdateEndIfGreater(currentPosition.WriteTo(buffer, blockSize));

    public LinkedListPosition UpdateEndIfGreater(LinkedListPosition newPos)
    {
        if (newPos.GlobalPosition > endPosition.GlobalPosition)
            endPosition = newPos;
        return newPos;
    }

    public void Truncate(long value)
    {
        endPosition = AsSequence().GetPosition(value);
    }

    public void EnsureHasLocation(long value)
    {
        UpdateEndIfGreater(EndPosition.ExtendTo(value, blockSize));
    }

    protected (LinkedListPosition, bool atEnd) LoadFrom(
        Stream stream, LinkedListPosition readpos)
    {
        var ret = readpos.GetMoreBytes(stream, blockSize);
        UpdateEndIfGreater(ret.bufferEnd);
        return ret;
    }
    protected async ValueTask<(LinkedListPosition, bool)> LoadFromAsync(
        Stream stream, LinkedListPosition readpos)
    {
        var ret = await readpos.GetMoreBytesAsync(stream, blockSize).CA();
        UpdateEndIfGreater(ret.bufferEnd);
        return ret;
    }

    public virtual bool DoneGrowing() => blockSize == 0;

    public virtual void HasReadTo(SequencePosition consumed)
    {
        // some descendants use the method to release buffers that will not be
        // reused.
    }

    public LinkedListPosition PositionAt(long position)
    {
        EnsureHasLocation(position);
        var node = StartPosition.Node!;
        while (node.NextNodeIndex() <= position)
        {
            node =node.Next as LinkedListNode ??
                  throw new InvalidOperationException("Should never be null");
        }
        return new (node, (int)(position - node.RunningIndex));
    }

    protected void PruneFromFront(SequencePosition consumed)
    {
        var oldStartPosition = StartPosition;
        StartPosition = consumed;
        oldStartPosition.ClearTo(consumed);
    }


    #region IMultiplexSource

    public override long Length => endPosition.GlobalPosition;

    protected override Stream ReadFromOverride(long position, CountedSourceTicket ticket)
    {
        var ret = MultiBufferStream.Create(this, false, ticket);
        if (position > 0) ret.Seek(position, SeekOrigin.Begin);
        return ret;
    }

    protected override IByteSource ReadFromPipeOverride(long position, long startingPosition, CountedSourceTicket ticket)
    {
        var ret = new LinkedListByteSource(this, ticket);
        if (position > 0)
        {
            var initial = PositionAt(position);
            ret.AdvanceTo(initial);
        }

        return ret.WithCurrentPosition(startingPosition);
    }

    protected override void CleanUp()
    {
        StartPosition.ClearTo(LinkedListPosition.NullPosition);
    }
    #endregion

}

internal abstract class LinkedList<T> : LinkedList where T : LinkedList<T>
{

}
