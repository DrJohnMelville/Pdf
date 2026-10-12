using System;
using System.Buffers;

namespace Melville.Parsing.IStreamAdaptors
{
    public readonly struct PooledByteBuffer(int minLength) : IDisposable
    {
        readonly byte[] bytes = ArrayPool<byte>.Shared.Rent(minLength);

        public void Dispose() => ArrayPool<byte>.Shared.Return(bytes);

        public ref byte this[int index] => ref bytes[index];
        public int Length => bytes.Length;
        public Span<byte> AsSpan() => bytes.AsSpan();
        public Memory<byte> AsMemory() => bytes.AsMemory();
        public static implicit operator byte[](PooledByteBuffer pab) => pab.bytes;
    }
}