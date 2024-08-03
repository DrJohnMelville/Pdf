using System;
using System.Buffers;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.LinkedLists;

internal partial class StreamBackedBuffer: LinkedList<StreamBackedBuffer>
{
    public static LinkedList Create(Stream source, int bufferSize = 4096)
    {
        return new StreamBackedBuffer(source).With(bufferSize);
    }

    [FromConstructor] private readonly Stream source;
}

internal class MultiBufferStreamList : LinkedList<MultiBufferStreamList>
{
    public static LinkedList WritableList(int blockSize) =>
        new MultiBufferStreamList().With(blockSize);
    public static LinkedList SingleItemList(ReadOnlyMemory<byte> source) =>
        new MultiBufferStreamList().With(source);

}

internal abstract class LinkedList<T> : LinkedList where T : LinkedList<T>
{

}

internal abstract class LinkedList
{
    public LinkedListPosition StartPosition { get; private set; }
    private LinkedListPosition endPosition;
    public LinkedListPosition EndPosition => endPosition;
    private int references;
    private int blockSize;

    protected internal LinkedList()
    {
    }

    public void AddReference() => references++;
    public void ReleaseReference() => references--;

    public LinkedList With(int blockSize)
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

    private ReadOnlySequence<byte> SequenceAfter(LinkedListPosition start) =>
        start.SequenceTo(EndPosition);

    public (int, LinkedListPosition) Read(LinkedListPosition origin, Span<byte> buffer) => 
        ReadCore(origin, buffer, BytesAvailableAfter(origin));


    protected virtual long BytesAvailableAfter(LinkedListPosition origin) => 
        endPosition.GlobalPosition - origin.GlobalPosition;

    public async ValueTask<(int, LinkedListPosition)> ReadAsync(
        LinkedListPosition origin, Memory<byte> buffer)
    {
        var bytesToRead = await BytesAvailableAfterAsync(origin).CA();
        return ReadCore(origin, buffer.Span, bytesToRead);
    }

    protected virtual ValueTask<long> BytesAvailableAfterAsync(LinkedListPosition origin) => 
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
        currentPosition.WriteTo(buffer, blockSize, ref endPosition);

    public void Truncate(long value)
    {
        endPosition = AsSequence().GetPosition(value);
    }

    public long Length() => EndPosition.GlobalPosition;

    public void EnsureHasLocation(long value)
    {
        endPosition = EndPosition.ExtendTo(value, blockSize);
    }
}