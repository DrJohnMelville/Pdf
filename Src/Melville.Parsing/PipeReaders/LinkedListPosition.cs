using System.Diagnostics;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ObjectRentals;

namespace Melville.Parsing.PipeReaders;

internal readonly partial struct LinkedListPosition
{
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
        new LinkedListPosition((LinkedListNode)sp.GetObject(), sp.GetInteger());

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

    public static bool operator==(in LinkedListPosition a, in LinkedListPosition b) =>
        a.Node== b.Node && a.Index == b.Index;

    public static bool operator !=(LinkedListPosition a, LinkedListPosition b) => !(a == b);

    public ValueTask<(LinkedListPosition bufferEnd, bool atSourceEnd)> 
        GetMoreBytesAsync(Stream stream, int desiredLength) =>
        NextWriteTarget(desiredLength).FillBufferFromAsync(stream);

    private LinkedListPosition NextWriteTarget(int desiredLength) => 
        (NodeHasEmptySpace() ? this: new(CreateNewNode(desiredLength), 0));

    private bool NodeHasEmptySpace() => Index < Node.LocalLength;

    private LinkedListNode CreateNewNode(int desiredLength)
    {
        var newNode = ObjectPool<LinkedListNode>.Shared.Rent()
            .With(desiredLength);
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
        GetMoreBytes(Stream stream, int desiredBufferSize)=>
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
}