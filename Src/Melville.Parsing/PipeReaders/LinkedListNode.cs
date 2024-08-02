using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ObjectRentals;

namespace Melville.Parsing.PipeReaders
{
    internal class LinkedListNode : ReadOnlySequenceSegment<byte>, IClearable
    {
        public static readonly LinkedListNode Empty = new LinkedListNode();
        public static LinkedListNode Rent(int length) =>
            ObjectPool<LinkedListNode>.Shared.Rent().With(length);


        private byte[] buffer = [];

        public int LocalLength => buffer.Length;
        
        public LinkedListNode With(int desiredLength, LinkedListNode? next = null)
        {
            Debug.Assert(desiredLength > 0);
            return With(ArrayPool<byte>.Shared.Rent(desiredLength), next);
        }

        public LinkedListNode With (byte[] buffer, LinkedListNode? next)
        {
            this.buffer = buffer;
            Memory = buffer;
            Next = next;
            RunningIndex = 0;
            return this;
        }

        public void Clear()
        {
            ArrayPool<byte>.Shared.Return(buffer);  
            Memory = ReadOnlyMemory<byte>.Empty;
            Next = null;
            buffer = [];
        }

        public void Append(LinkedListNode next)
        {
            Debug.Assert(next.LocalLength > 0);
            Next = next;
            next.RunningIndex = RunningIndex + buffer.Length;
        }

        public ValueTask<int> FillFromAsync(Stream s, int startAt) =>
            s.ReadAsync(buffer.AsMemory(startAt));

        public int FillFrom(Stream stream, int index) => 
            stream.Read(buffer.AsSpan(index));

        public int FillFrom(ReadOnlySpan<byte> source, int index)
        {
            var target = buffer.AsSpan(index);
            var length = Math.Min(target.Length, source.Length);
            source[..length].CopyTo(target);
            return length;
        }

        public void RenumberStartingPosition(long startAt)
        {
            RunningIndex = startAt;
            if (Next is LinkedListNode lln)
                lln.RenumberStartingPosition(startAt+buffer.Length);
        }
    }
}