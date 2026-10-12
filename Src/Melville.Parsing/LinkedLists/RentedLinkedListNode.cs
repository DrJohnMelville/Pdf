using Melville.StreamInterfaces;
using System.Buffers;

namespace Melville.Parsing.LinkedLists;

internal class RentedLinkedListNode: LinkedListNode
{
    private readonly byte[] buffer;

    public RentedLinkedListNode(byte[] rentedBuffer) : 
        base(new ReadOnlyMemory<byte>(rentedBuffer))
    {
        buffer = rentedBuffer;
    }

    public override void Dispose()
    {
        ArrayPool<byte>.Shared.Return(buffer);
        base.Dispose();
    }
    public override ValueTask<int> FillFromAsync(Stream s, int startAt) => 
        s.ReadAsync(buffer.AsMemory(startAt));

    public override int FillFrom(Stream stream, int index) => 
        stream.Read(buffer.AsSpan(index));

    public override int FillFrom(ReadOnlySpan<byte> source, int index)
    {
        var target = buffer.AsSpan(index);
        source.TakeMax(target.Length, out var len).CopyTo(target);
        return len;
    }
}