using Melville.Parsing;
using Melville.Parsing.IStreamAdaptors;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Diagnostics;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal sealed class Ascii85SpanTokenizer : SpanTokenizer
{
    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
    {
        length = source.IndexOf("~>"u8) + 2;
        return length > 2;
    }
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source)
    {
        var input = new SpanParsingBuffer(source[2..^2]);
        var buffer = new PooledBuffer<byte>(source.Length);
        var destination = buffer.AsSpan();
        var core = new Ascii85DecoderCore();
        while (!core.InnerConvert(ref input, ref destination, true))
        {
            var len = GetLength(buffer, destination);
            var newBuffer = new PooledBuffer<byte>(buffer.Length * 2);
            buffer.AsSpan().CopyTo(newBuffer.AsSpan());
            destination = newBuffer.AsSpan()[len..];
            buffer.Dispose();
            buffer = newBuffer;
        }
        ;
        var length = 
            GetLength(buffer, destination);
        var ret = PostscriptValueFactory.CreateString(buffer.AsSpan()[..length], StringKind.String);
        buffer.Dispose();
        return ret;
    }

    private static int GetLength(PooledBuffer<byte> buffer, Span<byte> destination) =>
        buffer.Length - destination.Length;

}
