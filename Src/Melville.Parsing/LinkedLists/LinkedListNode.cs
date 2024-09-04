using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Melville.Parsing.ObjectRentals;

namespace Melville.Parsing.LinkedLists;

internal class LinkedListNode : ReadOnlySequenceSegment<byte>, IDisposable
{
    public static readonly LinkedListNode Empty = new LinkedListNode(
        ReadOnlyMemory<byte>.Empty);

    public static LinkedListNode BufferNode(int length) =>
        new RentedLinkedListNode(ArrayPool<byte>.Shared.Rent(length));
    public static LinkedListNode ReadOnlyNode(ReadOnlyMemory<byte> source) =>
        new LinkedListNode(source);

    public LinkedListNode(ReadOnlyMemory<byte> source) => Memory = source;

    public int LocalLength => Memory.Length;

    public virtual void Dispose()
    {
    }

    public void Append(LinkedListNode next)
    {
        Debug.Assert(next.LocalLength > 0);
        Next = next;
        next.RunningIndex = NextNodeIndex();
    }

    public long NextNodeIndex() => RunningIndex + Memory.Length;
    
    public virtual ValueTask<int> FillFromAsync(Stream s, int startAt) => 
        throw new InvalidOperationException("Cannot write to a read only node");

    public virtual int FillFrom(Stream stream, int index) => 
        throw new InvalidOperationException("Cannot write to a read only node");

    public virtual int FillFrom(ReadOnlySpan<byte> source, int index) => 
        throw new InvalidOperationException("Cannot write to a read only node");
}