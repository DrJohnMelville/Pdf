using Melville.StreamInterfaces;
using System.Diagnostics;
using System.Text;

namespace Melville.Fonts.Type1TextParsers.EexecDecoding;

internal class DecodingReader(IAsyncReader inner, ushort key) : IAsyncReader
{
    private ushort key = key;
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        int read = await inner.ReadAsync(buffer, cancellation).CA();
        DecodeType1Encoding.DecodeSegment(buffer.Span[..read], ref key);
        return read;
    }
}