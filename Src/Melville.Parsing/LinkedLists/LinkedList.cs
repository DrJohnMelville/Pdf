using System;
using System.Buffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.LinkedLists;

internal readonly struct LinkedListBehavior
{

}

internal class LinkedList
{
    private LinkedListBehavior behavior = default;
    public LinkedListPosition StartPosition { get; private set; }
    private LinkedListPosition endPosition;
    public LinkedListPosition EndPosition => endPosition;
    private int references;
    private int blockSize;

    private LinkedList()
    {
    }

    public static LinkedList WritableList(int blockSize) =>
        new LinkedList().With(blockSize);
    public static LinkedList SingleItemList(ReadOnlyMemory<byte> source) =>
        new LinkedList().With(source);

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

    public int Read(ref LinkedListPosition origin, Span<byte> buffer)
    {
        var seq = SequenceAfter(origin);
        var length = Math.Min(seq.Length, buffer.Length);
        var reader = new SequenceReader<byte>(seq);
        reader.TryCopyTo(buffer.Slice(0, (int)length));
        origin = seq.GetPosition(length);
        return (int) length;
    }

    public LinkedListPosition Write(
        LinkedListPosition currentPosition, ReadOnlySpan<byte> buffer) =>
        currentPosition.WriteTo(buffer, blockSize, ref endPosition);

    public void Truncate(long value)
    {
        endPosition = AsSequence().GetPosition(value);
    }

    public long Length() => EndPosition.GlobalPosition;
}