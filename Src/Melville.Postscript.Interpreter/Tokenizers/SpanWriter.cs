using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal ref struct SpanWriter(int size) : IDisposable
    {
        private byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
        private int length = 0;
        public void Dispose() => ArrayPool<byte>.Shared.Return(buffer);

        public void Push(byte b)
        {
            TryExpandBuffer();
            buffer[length++] = b;
        }
        void TryExpandBuffer()
        {
            if (length < buffer.Length) return;
            var newBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
            buffer.CopyTo(newBuffer);
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = newBuffer;
        }

        public Span<byte> Value => buffer.AsSpan(0, length);
    }
}
