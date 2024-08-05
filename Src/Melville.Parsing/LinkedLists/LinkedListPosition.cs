using System.Buffers;
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



    public (LinkedListPosition bufferEnd, bool atSourceEnd)
        GetMoreBytes(Stream stream, int desiredBufferSize) =>
        NextWriteTarget(desiredBufferSize).FillBufferFrom(stream);
     
    public ValueTask<(LinkedListPosition bufferEnd, bool atSourceEnd)>
        GetMoreBytesAsync(Stream stream, int desiredLength) =>
        NextWriteTarget(desiredLength).FillBufferFromAsync(stream);

    private LinkedListPosition NextWriteTarget(int desiredLength) =>
        (NodeHasEmptySpace() ? this : StartOfNextBlock(desiredLength));

    private LinkedListPosition StartOfNextBlock(int desiredLength) =>
        Node.Next is null
            ? new(CreateNewNode(desiredLength), 0)
            : new LinkedListPosition((LinkedListNode)Node.Next, 0);

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



    private (LinkedListPosition bufferEnd, bool atSourceEnd) FillBufferFrom(Stream stream)
    {
        var bytesRead = Node.FillFrom(stream, Index);
        return (
            new LinkedListPosition(Node, Index + bytesRead),
            bytesRead == 0);
    }

    public long GlobalPosition => Node.RunningIndex + Index;

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
        ReadOnlySpan<byte> buffer, int blockSize)
    {
        var extendedPos = ExtendTo(GlobalPosition + buffer.Length, blockSize);
        CopyDataToList(buffer);
        return extendedPos;
    }

    private void CopyDataToList(ReadOnlySpan<byte> buffer)
    {
        var position = this;
        FillCurrentNode(ref buffer);
        while (buffer.Length > 0)
        {
            position = FindNextNode(position);
            position.FillCurrentNode(ref buffer);
        }
    }

    private static LinkedListPosition FindNextNode(LinkedListPosition position) => 
        new((LinkedListNode)position.Node.Next!, 0);

    private void FillCurrentNode(ref ReadOnlySpan<byte> buffer) => 
        buffer = buffer.Slice(Node.FillFrom(buffer, Index));

    public LinkedListPosition ExtendTo(long value, int blockSize)
    {
        if (value <= GlobalPosition) return this;
        if (blockSize == 0)
            throw new ArgumentOutOfRangeException(nameof(blockSize), 
                "Cannot extend a read only Linked List");
        var node = this;
        while (value - node.GlobalPosition is > 0 and var delta)
        {
            node = (delta) switch
            {
                <= 0 => node,
                var x when x <= node.SpaceInCurrentNode =>
                    new LinkedListPosition(node.Node, node.Index + (int)x),
                _ => node.StartOfNextBlock(blockSize)

            };
        }
        return node;
    }

    private int SpaceInCurrentNode => Node.LocalLength - Index;
}