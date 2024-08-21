using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Fonts.Type1TextParsers.EexecDecoding;

internal class EexecDecodeStream(Stream source, ushort pkey) 
    : DefaultBaseStream(true, false, true)
{
    public override int Read(Span<byte> buffer)
    {
        var ret = source.Read(buffer);
        DecodeType1Encoding.DecodeSegment(buffer[..ret], ref pkey);
        return ret;
    }
    
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var ret = await source.ReadAsync(buffer, cancellationToken).CA();
        DecodeType1Encoding.DecodeSegment(buffer.Span[..ret], ref pkey);
        return ret;
    }

    protected override void Dispose(bool disposing)
    {
        source.Dispose();
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await source.DisposeAsync().CA();
        await base.DisposeAsync().CA();
    }
}