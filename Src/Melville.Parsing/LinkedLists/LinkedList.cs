using System;
using System.Buffers;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.LinkedLists;

internal abstract class LinkedList: IMultiplexSource
{
    public LinkedListPosition StartPosition { get; private set; }
    private LinkedListPosition endPosition;
    public LinkedListPosition EndPosition => endPosition;
    protected int references;
    private int blockSize;

    protected internal LinkedList()
    {
    }

    public virtual void AddReference() => references++;
    public void ReleaseReference() => references--;

    public virtual LinkedList With(int blockSize)
    {
        this.blockSize = blockSize;
        LinkedListNode firstNode = CreateNewBlock();
        references = 1;
        StartPosition = endPosition = new LinkedListPosition(firstNode, 0);
        return this;
    }

    public LinkedList With(ReadOnlyMemory<byte> source)
    {
        LinkedListNode firstNode = LinkedListNode.Rent(source);
        blockSize = 0;
        references = 1;
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

    public long Length() => EndPosition.GlobalPosition;

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

    Stream IMultiplexSource.ReadFrom(long position)
    {
        var ret = new MultiBufferStream(this, true, false, true);
        if (position > 0) ret.Seek(position, SeekOrigin.Begin);
        return ret;
    }

    IByteSource IMultiplexSource.ReadPipeFrom(long position, long startingPosition)
    {
        var ret = new LinkedListByteSource(this);
        if (position > 0)
        {
            var initial = PositionAt(position);
            ret.AdvanceTo(initial);
        }

        return ret.WithCurrentPosition(startingPosition);
    }

    public void Dispose()
    
    {
    }

    long IMultiplexSource.Length => endPosition.GlobalPosition;
}

internal abstract class LinkedList<T> : LinkedList where T : LinkedList<T>
{

}
