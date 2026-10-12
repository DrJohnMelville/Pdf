using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;
using System.Diagnostics;

namespace Melville.Fonts.Type1TextParsers.EexecDecoding
{
    public class HexDecodingReader(IAsyncReader inner) : IAsyncReader
    {
        byte? savedByte = null;
        public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
        {
            int maxDesired = buffer.Length * 2;
            using var hexDigitBuff = new PooledByteBuffer(maxDesired);
            var actualDigitLen = await GetDigitsAsync(hexDigitBuff.AsMemory()[..maxDesired]);
            return CopyHexData(hexDigitBuff.AsSpan()[..actualDigitLen], buffer.Span);
        }
        async ValueTask<int> GetDigitsAsync(Memory<byte> target)
        {
            if (savedByte is { } sb)
            {
                target.Span[0] = sb;
                return 1 + await inner.ReadAsync(target[1..]);
            }else
            {
                return await inner.ReadAsync(target);
            }
        }
        int CopyHexData(Span<byte> source, Span<byte> target)
        {
            Debug.Assert(source.Length <= (2 * target.Length));
            var digits = new SpanParsingBuffer(source);
            int nextOutput = 0;
            while (digits.TryGetNext(CharacterClassifier.HexDigits, out var byte1))
            {
                if (digits.TryGetNext(CharacterClassifier.HexDigits, out byte byte2))
                {
                    target[nextOutput++] = CharacterClassifier.ByteFromHexChars(byte1, byte2);
                }
                else
                {
                    savedByte = byte1;
                    return nextOutput;
                }
            }
            savedByte = null;
            return nextOutput;
        }
    }
}

#warning move this deeper in the stack
public readonly struct PooledByteBuffer(int minLength): IDisposable
{
    readonly byte[] bytes = ArrayPool<byte>.Shared.Rent(minLength);

    public void Dispose() => ArrayPool<byte>.Shared.Return(bytes);

    public ref byte this[int index] => ref bytes[index];
    public int Length => bytes.Length;
    public Span<byte> AsSpan() => bytes.AsSpan();
    public Memory<byte> AsMemory() => bytes.AsMemory();
    public static implicit operator byte[](PooledByteBuffer pab) => pab.bytes;
}