﻿using System.Buffers;
using System.Diagnostics;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ObjectRentals;

namespace Melville.Parsing.LinkedLists;

internal readonly partial struct LinkedListPosition
{
    public static readonly LinkedListPosition NullPosition = new(LinkedListNode.Empty, 0);

    [FromConstructor] public LinkedListNode Node { get; }
    [FromConstructor] public int Index { get; }

#if DEBUG
    partial void OnConstructed()
    {
        Debug.Assert(Index <= Node.Memory.Length);
    }
#endif

    public static implicit operator SequencePosition(LinkedListPosition llp) =>
        new SequencePosition(llp.Node, llp.Index);

    public static implicit operator LinkedListPosition(SequencePosition sp) =>
        new(sp.GetObject() as LinkedListNode ?? LinkedListNode.Empty, sp.GetInteger());

    public void ClearTo(LinkedListPosition target)
    {
        var current = Node;
        while (current != null && current != target.Node)
        {
            var next = (LinkedListNode?)current.Next;
            ObjectPool<LinkedListNode>.Shared.Return(current);
            current = next;
        }
    }

    public override bool Equals(object? obj) =>
        obj is LinkedListPosition other && this == other;

    public override int GetHashCode() => HashCode.Combine(Node, Index);

    public static bool operator ==(in LinkedListPosition a, in LinkedListPosition b) =>
        a.Node == b.Node && a.Index == b.Index;

    public static bool operator !=(LinkedListPosition a, LinkedListPosition b) => !(a == b);

    public ValueTask<(LinkedListPosition bufferEnd, bool atSourceEnd)>
        GetMoreBytesAsync(Stream stream, int desiredLength) =>
        NextWriteTarget(desiredLength).FillBufferFromAsync(stream);

    private LinkedListPosition NextWriteTarget(int desiredLength) =>
        (NodeHasEmptySpace() ? this : StartOfNewBlock(desiredLength));

    private LinkedListPosition StartOfNewBlock(int desiredLength) => 
        new(CreateNewNode(desiredLength), 0);

    private bool NodeHasEmptySpace() => Index < Node.LocalLength;

    private LinkedListNode CreateNewNode(int desiredLength)
    {
        var newNode = LinkedListNode.Rent(desiredLength);
        Node.Append(newNode);
        return newNode;
    }

    private async ValueTask<(LinkedListPosition bufferEnd, bool atSourceEnd)>
        FillBufferFromAsync(Stream stream)
    {
        var bytesRead = await Node.FillFromAsync(stream, Index).CA();
        return (
            new LinkedListPosition(Node, Index + bytesRead),
            bytesRead == 0);
    }

    public (LinkedListPosition bufferEnd, bool atSourceEnd)
        GetMoreBytes(Stream stream, int desiredBufferSize) =>
        NextWriteTarget(desiredBufferSize).FillBufferFrom(stream);

    private (LinkedListPosition bufferEnd, bool atSourceEnd) FillBufferFrom(Stream stream)
    {
        var bytesRead = Node.FillFrom(stream, Index);
        return (
            new LinkedListPosition(Node, Index + bytesRead),
            bytesRead == 0);
    }

    public long GlobalPosition => Node.RunningIndex + Index;

    public void RenumberCurrentPosition(long startAt)
    {
        Node.RenumberStartingPosition(startAt - Index);
    }

    public ReadOnlySequence<byte> SequenceTo(LinkedListPosition endPosition)
    {
        var first = AdjustEndOfSegment();
        return new ReadOnlySequence<byte>(first.Node, first.Index,
            endPosition.Node, endPosition.Index);
    }

    public LinkedListPosition AdjustEndOfSegment() =>
        (Index == Node.LocalLength && Node.Next is LinkedListNode nextNode)
            ? new LinkedListPosition(nextNode, 0)
            : this;

    public LinkedListPosition WriteTo(
        ReadOnlySpan<byte> buffer, int blockSize, ref LinkedListPosition endPosition)
    {
        var consumed = FillCurrentNode(ref buffer);
        if (buffer.Length == 0)
        {
            var newPosition = new LinkedListPosition(Node, Index + consumed);
            if (newPosition.Node == endPosition.Node && 
                    newPosition.Index > endPosition.Index)
                endPosition = newPosition;
            return newPosition;
        }

        if (Node.Next is LinkedListNode next)
            return new LinkedListPosition(next,0).WriteTo(
                buffer, blockSize, ref endPosition);

        return endPosition = StartOfNewBlock(blockSize).Append(buffer, blockSize);
    }

    private int FillCurrentNode(ref ReadOnlySpan<byte> buffer)
    {
        var ret = Node.FillFrom(buffer, Index);
        buffer = buffer.Slice(ret);
        return ret;
    }

    public LinkedListPosition Append(ReadOnlySpan<byte> buffer, int blockSize)
    {
        var consumed = FillCurrentNode(ref buffer);
        return buffer.Length == 0 ? 
            new LinkedListPosition(Node, Index + consumed) : 
            StartOfNewBlock(blockSize).Append(buffer, blockSize);
    }
}