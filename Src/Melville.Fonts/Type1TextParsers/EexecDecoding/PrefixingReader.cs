using Melville.StreamInterfaces;

namespace Melville.Fonts.Type1TextParsers.EexecDecoding
{
    public class PrefixingReader(IAsyncReader inner, byte[] prefix) : IAsyncReader
    {
        private Memory<byte> prefix = prefix.AsMemory();

        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
        {
            if (prefix.Length > 0)
            {
                var len = Math.Min(prefix.Length, buffer.Length);
                prefix.Span[..len].CopyTo(buffer.Span);
                prefix = prefix[len..];
                return ValueTask.FromResult(len);
            }
            return inner.ReadAsync(buffer, cancellation);
        }
    }
}
