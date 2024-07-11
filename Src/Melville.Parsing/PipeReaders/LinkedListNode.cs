using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ObjectRentals;

namespace Melville.Parsing.PipeReaders
{
    internal class LinkedListNode : ReadOnlySequenceSegment<byte>, IClearable
    {
        private byte[] buffer = [];

        public int LocalLength => buffer.Length;
        
        public LinkedListNode With(int desiredLength, LinkedListNode next = null)
        {
            Debug.Assert(desiredLength > 0);
            buffer = ArrayPool<byte>.Shared.Rent(desiredLength);
            Memory = buffer;
            Next = next;
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
    }
}