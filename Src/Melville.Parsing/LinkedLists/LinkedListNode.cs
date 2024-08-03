using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ObjectRentals;

namespace Melville.Parsing.LinkedLists
{
    internal class LinkedListNode : ReadOnlySequenceSegment<byte>, IClearable
    {
        public static readonly LinkedListNode Empty = new LinkedListNode();
        public static LinkedListNode Rent(int length) =>
            ObjectPool<LinkedListNode>.Shared.Rent().With(length);
     public static LinkedListNode Rent(ReadOnlyMemory<byte> source) =>
            ObjectPool<LinkedListNode>.Shared.Rent().With(source);


        private byte[]? buffer = null;

        public int LocalLength => Memory.Length;

        private LinkedListNode With(int desiredLength)
        {
            Debug.Assert(desiredLength > 0);
            buffer = ArrayPool<byte>.Shared.Rent(desiredLength);
            return With(buffer);
        }

        private LinkedListNode With(ReadOnlyMemory<byte> memory)
        {
            Memory = memory;
            Next = null;
            RunningIndex = 0;
            return this;
        }

        public void Clear()
        {
            if (buffer is not null)
                ArrayPool<byte>.Shared.Return(buffer);
            Memory = ReadOnlyMemory<byte>.Empty;
            Next = null;
            buffer = [];
        }

        public void Append(LinkedListNode next)
        {
            Debug.Assert(next.LocalLength > 0);
            Next = next;
            next.RunningIndex = RunningIndex + Memory.Length;
        }

        [MemberNotNull(nameof(buffer))]
        private void AssertWritableNode()
        {
            if (buffer is null)
                throw new InvalidOperationException("Cannot write to a read only node");
        }

        public ValueTask<int> FillFromAsync(Stream s, int startAt)
        {
            AssertWritableNode();
            return s.ReadAsync(buffer.AsMemory(startAt));
        }

        public int FillFrom(Stream stream, int index)
        {
            AssertWritableNode();
            return stream.Read(buffer.AsSpan(index));
        }

        public int FillFrom(ReadOnlySpan<byte> source, int index)
        {
            AssertWritableNode();
            var target = buffer.AsSpan(index);
            var length = Math.Min(target.Length, source.Length);
            source[..length].CopyTo(target);
            return length;
        }

        public void RenumberStartingPosition(long startAt)
        {
            RunningIndex = startAt;
            if (Next is LinkedListNode lln)
                lln.RenumberStartingPosition(startAt + buffer.Length);
        }
    }
}